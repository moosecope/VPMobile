using GTG.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace VP_Mobile.Models
{

    public class GPSMessageRecievedEventArgs : EventArgs
    {
        public Exception Error { get; private set; }

        public String OriginalMessage { get; private set; }

        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        public bool ErrorInitializing { get; private set; }

        public GPSMessageRecievedEventArgs(double latitude, double longitude, String message)
        {
            Latitude = latitude;
            Longitude = longitude;
            OriginalMessage = message;
        }

        public GPSMessageRecievedEventArgs(Exception error, bool initial)
        {
            Error = error;
            ErrorInitializing = initial;
        }
    }

    public class GPSListener : IDisposable
    {
        public static readonly char[] GPS_MESSAGE_SPLITTERS = new char[] { '\n', '\r', '⌈', '⌋', '\u0003' };

        private bool _isUdp;
        public bool IsUdp
        {
            get { return _isUdp; }
            set
            {
                _isUdp = value;
            }
        }
        
        private int _baudRate;
        public int BaudRate
        {
            get { return _baudRate; }
            set
            {
                _baudRate = value;
            }
        }
        
        private String _port;
        public String Port
        {
            get { return _port; }
            set
            {
                _port = value;
            }
        }

        private bool _stop;
        private Thread _gpsThread;
        private StringBuilder _gpsMessageBuilder;

        private SerialPort _serialPort;
        private UdpClient _udpPort;

        public event EventHandler<GPSMessageRecievedEventArgs> GPSMessageRecieved;

        public GPSListener(String port, int baudRate, bool isUdp)
        {
            _isUdp = isUdp;
            _baudRate = baudRate;
            _port = port;
        }

        public static Point DecodeMessage(String message)
        {
            Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(message), message }
                    });
            Point ret = new Point();

            if (String.IsNullOrWhiteSpace(message))
                return null;

            String unitId = null, deviceId = null, strLat = null, strLong = null;
            double lat = 0, lng = 0, speed = 0;
            if (message.IndexOf("cad4car", 0, 10, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                try
                {
                    message = message.Substring(message.IndexOf("cad4car"));
                }
                catch (Exception) { }
                if (message.Length < 115)
                    throw new ArgumentException("HMAP message is too short - " + message);

                unitId = message.Substring(49, 16).Trim().ToUpper();
                deviceId = message.Substring(20, 16).Trim().ToUpper();

                if (message[97] == 'N')
                {
                    strLat = message.Substring(98, 8);
                    lat = double.Parse(strLat) / 360000;
                }
                else
                {
                    strLat = message.Substring(97, 8);
                    lat = -double.Parse(strLat) / 360000;
                }
                if (message[107] == 'N')
                {
                    strLong = message.Substring(109, 8);
                    lng = double.Parse(strLong) / 360000;
                }
                else
                {
                    strLong = message.Substring(108, 8);
                    lng = -double.Parse(strLong) / 360000;
                }
            }
            else if(message.IndexOf("SGAVL", 0, 10, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                var xDoc = new XmlDocument();
                xDoc.LoadXml(message.Substring(1, message.Length - 2));

                var xNode = xDoc.SelectSingleNode("//ID/UNT");
                if(xNode != null)
                    unitId = xNode.InnerText;

                xNode = xDoc.SelectSingleNode("//ID/CMP");
                if (xNode != null)
                    deviceId = xNode.InnerText;

                xNode = xDoc.SelectSingleNode("//GPS/LAT");
                if (xNode != null)
                {
                    strLat = xNode.InnerText;
                    lat = double.Parse(strLat);
                }

                xNode = xDoc.SelectSingleNode("//GPS/LON");
                if (xNode != null)
                {
                    strLong = xNode.InnerText;
                    lng = double.Parse(strLong);
                }

                xNode = xDoc.SelectSingleNode("//GPS/MPH");
                if (xNode != null)
                    speed = double.Parse(xNode.InnerText);
            }
            else if(message.IndexOf("gtgavldd", 0, 8, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                var strsplit = message.Split('|');
                if (strsplit.Length != 4)
                    throw new ArgumentException("GTG AVL message length is incorrect - " + message);

                unitId = strsplit[1].Trim().ToUpper();

                strLat = strsplit[2];
                lat = double.Parse(strLat);
                strLong = strsplit[3];
                lng = -Math.Abs(double.Parse(strLong));
            }
            else if (message.IndexOf("gtgavldm", 0, 8, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                var strsplit = message.Split('|');
                if (strsplit.Length != 4)
                    throw new ArgumentException("GTG AVL message length is incorrect - " + message);

                unitId = strsplit[1].Trim().ToUpper();

                var strDec = strsplit[2].Split('.');
                var tmp = strDec[0].Trim();
                var strMin = tmp.Substring(tmp.Length - 2);
                var strDeg = tmp.Substring(0, tmp.Length - strMin.Trim().Length);

                lat = double.Parse(strDeg) + double.Parse(strMin + "." + strDec[1]) / 60;

                strDec = strsplit[2].Split('.');
                tmp = strDec[0].Trim();
                strMin = tmp.Substring(tmp.Length - 2);
                strDeg = tmp.Substring(0, tmp.Length - strMin.Trim().Length);

                lng = -Math.Abs(double.Parse(strDeg) + double.Parse(strMin + "." + strDec[1]) / 60);
            }
            else if(message.IndexOf("gtgvision", 0, 9, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                var strsplit = message.Split('|');
                if (strsplit.Length != 6)
                    throw new ArgumentException("GTG AVL message length is incorrect - " + message);

                deviceId = strsplit[1].Trim().ToUpper();

                unitId = strsplit[2].Trim().ToUpper();

                lat = double.Parse(strsplit[3]);
                lng = -Math.Abs(double.Parse(strsplit[4]));

                speed = double.Parse(strsplit[5]);
            }
            else
            {
                var strMsg = message.Replace(">", "");
                strMsg = strMsg.Replace("<", "");

                var strSplit = strMsg.Split(new[] { ";", Environment.NewLine }, StringSplitOptions.None);
                foreach (var msg in strSplit)
                {
                    var strSplit2 = msg.Split(',');

                    if (strSplit2[0].Trim().ToUpper() == "$DEVID")
                        unitId = strSplit2[1].Trim().ToUpper();
                    else if (strSplit2[0].Trim().ToUpper() == "$GPGGA") //$GPGGA,144858.00,3951.86595,N,08407.81407,W,1,07,1.24,00300,M,-033,M,,*5C
                    {
                        strLat = strSplit2[2];
                        var strLatDir = strSplit2[3];
                        strLong = strSplit2[4];
                        var strLongDir = strSplit2[5];

                        if (!String.IsNullOrWhiteSpace(strLat))
                        {
                            var strDec = strLat.Split('.');
                            var tmp = strDec[0].Trim();
                            var strMin = tmp.Substring(tmp.Length - 2);
                            var strDeg = tmp.Substring(0, tmp.Length - strMin.Trim().Length);

                            lat = double.Parse(strDeg) + double.Parse(strMin + "." + strDec[1]) / 60;
                        }

                        if (!String.IsNullOrWhiteSpace(strLong))
                        {
                            var strDec = strLong.Split('.');
                            var tmp = strDec[0].Trim();
                            var strMin = tmp.Substring(tmp.Length - 2);
                            var strDeg = tmp.Substring(0, tmp.Length - strMin.Trim().Length);

                            lng = -Math.Abs(double.Parse(strDeg) + double.Parse(strMin + "." + strDec[1]) / 60);
                        }
                    }
                    else if (strSplit2[0].StartsWith("ID"))
                    {
                        var tmp = strSplit2[0].Trim().ToUpper();
                        tmp = tmp.Replace("ID", "");
                        tmp = tmp.Replace("=", "");

                        unitId = tmp;
                    }
                    else if (strSplit2[0].StartsWith("RLN"))
                    {
                        var tmp = strSplit2[0].Trim().ToUpper();
                        tmp = tmp.Replace("RLN", "").Trim();

                        lat = double.Parse(tmp.Substring(9, 3) + "." + tmp.Substring(12, 7));
                        lng = double.Parse(tmp.Substring(19, 4) + "." + tmp.Substring(18, 5));
                    }
                    else if (strSplit2[0].StartsWith("RPV"))
                    {
                        var tmp = strSplit2[0].Trim().ToUpper();
                        var delimeters = new char[] { '+', '-' };
                        var idxfst = tmp.IndexOfAny(delimeters);
                        var idxlst = tmp.LastIndexOfAny(delimeters);
                        strLat = tmp.Substring(idxfst, idxlst - idxfst);
                        strLong = tmp.Substring(idxlst, tmp.Length - idxlst);

                        lat = double.Parse(strLat.Substring(0, 3) + "." + strLat.Substring(3, strLat.Length - 3));
                        lng = double.Parse(strLong.Substring(0, 4) + "." + strLong.Substring(4, strLong.Length - 4));
                    }
                    else
                        return null;
                }
            }
            ret.Latitude = lat;
            ret.Longitude = lng;
            Logging.LogMessage(Logging.LogType.Info, "GPS Message '" + message + "' Decoded - Lat:'" + lat + "' Long: '" + lng + "' Speed: '" + speed + "' Unit: '" + unitId + "' Device: '" + deviceId + "'");
            return ret;
        }

        public void Start()
        {
            Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
            if (_gpsThread == null || !_gpsThread.IsAlive)
            {
                _gpsThread = new Thread(StartGPS);
                _gpsThread.IsBackground = true;
                _gpsThread.Start();
            }
        }

        public void Stop()
        {
            _stop = true;
        }

        private void StartGPS()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                _stop = false;
                if (_isUdp)
                {
                    using (_udpPort = new UdpClient(int.Parse(_port)))
                    {
                        var endPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
                        while (!_stop)
                        {
                            try
                            {
                                byte[] bytes = _udpPort.Receive(ref endPoint);
                                HandleMessage(bytes, bytes.Length);
                            }
                            catch (Exception ex)
                            {
                                GPSMessageRecieved?.Invoke(this, new GPSMessageRecievedEventArgs(ex, false));
                            }
                        }
                    }
                }
                else
                {
                    using (_serialPort = new SerialPort(_port, _baudRate))//, Parity.None, 8, StopBits.One
                    {
                        _serialPort.Open();
                        byte[] read = new byte[_serialPort.ReadBufferSize];
                        int bytesRead = 0;
                        while (!_stop && _serialPort.IsOpen)
                        {
                            try
                            {
                                if ((bytesRead = _serialPort.Read(read, 0, read.Length)) > 0)
                                    HandleMessage(read, bytesRead);
                            }
                            catch (Exception ex)
                            {
                                GPSMessageRecieved?.Invoke(this, new GPSMessageRecievedEventArgs(ex, false));
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                GPSMessageRecieved?.Invoke(this, new GPSMessageRecievedEventArgs(ex, true));
            }
        }

        private void HandleMessage(byte[] read, int bytesRead)
        {
            Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(read), read },
                        { nameof(bytesRead), bytesRead }
                    });
            string message = Encoding.ASCII.GetString(read, 0, bytesRead);
            Logging.LogMessage(Logging.LogType.Info, "GPS Message Recieved - " + message);
            if (_gpsMessageBuilder == null)
                _gpsMessageBuilder = new StringBuilder();
            _gpsMessageBuilder.Append(message);
            int remove = -1;
            for (int i = 0; i < _gpsMessageBuilder.Length; i++)
            {
                if (GPS_MESSAGE_SPLITTERS.Contains(_gpsMessageBuilder[i]))
                {
                    Point ret = null;
                    String msg = null;
                    if (remove >= 0)
                        msg = _gpsMessageBuilder.ToString(remove, i - remove + 1);
                    else
                        msg = _gpsMessageBuilder.ToString(0, i + 1);
                    ret = DecodeMessage(msg);
                    if(ret != null)
                        GPSMessageRecieved?.Invoke(this, new GPSMessageRecievedEventArgs(ret.Latitude, ret.Longitude, msg));
                    remove = i;
                }
            }
            if (remove >= 0)
                _gpsMessageBuilder.Remove(0, remove + 1);
        }

        public void Dispose()
        {
            Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
            _stop = true;
            if (_udpPort != null)
            {
                try
                {
                    _udpPort.Close();
                }
                catch (Exception) { }
            }
            if (_serialPort != null)
            {
                try
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                }
                catch (Exception){}
            }
            if(_gpsThread.IsAlive)
            {
                try
                {
                    _gpsThread.Abort();
                }
                catch (Exception){}
            }
        }
    }
}
