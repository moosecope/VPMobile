using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using GTG.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VP_Mobile.StaticHelpers;

namespace VP_Mobile.ViewModels
{
    public class IncidentViewModel : Dictionary<String, Object>, INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public IncidentViewModel(Dictionary<string, Object> properties) : base(properties)
        {
        }
        #endregion

        #region public properties
        public BitmapSource CallTypeImage { get; set; }

        public String FirstDisplayItem
        {
            get
            {
                object ret;
                if (base.TryGetValue(FirstDisplayItemKey, out ret))
                    return ret?.ToString();
                else
                    return String.Empty;
            }
        }

        public String SecondDisplayItem
        {
            get
            {
                object ret;
                if (base.TryGetValue(SecondDisplayItemKey, out ret))
                    return ret?.ToString();
                else
                    return String.Empty;
            }
        }

        public Object UniqueID
        {
            get
            {
                object ret;
                if (base.TryGetValue(UNIQUE_ID, out ret))
                    return ret?.ToString();
                else
                    return String.Empty;
            }
        }

        public String UnitID
        {
            get
            {
                object ret;
                if (base.TryGetValue(UNIT_ID, out ret))
                    return ret?.ToString();
                else
                    return String.Empty;
            }
        }

        public String CallType
        {
            get
            {
                object ret;
                if (base.TryGetValue(CALL_TYPE, out ret))
                    return ret?.ToString();
                else
                    return String.Empty;
            }
        }

        public DateTime RecordTime
        {
            get
            {
                object ret;
                if (base.TryGetValue(RECORD_TIME, out ret) && ret is DateTime)
                    return (DateTime)ret;
                else
                    return DateTime.Now;
            }
        }

        private HashSet<String> _hiddenKeys = new HashSet<string> { FirstDisplayItemKey, SecondDisplayItemKey, RECORD_TIME, LATITUDE, LONGITUDE, "Rec_ID" };
        private ObservableCollection<Tuple<String, String>> _otherDisplayItems;
        public ObservableCollection<Tuple<String, String>> OtherDisplayItems
        {
            get
            {

                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_otherDisplayItems != null)
                        return _otherDisplayItems;
                    _otherDisplayItems = new ObservableCollection<Tuple<string, string>>();
                    foreach (var key in base.Keys.Where(key => !_hiddenKeys.Contains(key)))
                    {
                        _otherDisplayItems.Add(new Tuple<string, string>(key + ": ", base[key].ToString()));
                    }
                    return _otherDisplayItems;
                }
                catch (Exception ex)
                {
                    var message = "Error pulling OtherDisplayItems";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                    return _otherDisplayItems;
                }
            }
        }

        public double Latitude
        {
            get
            {
                object ret;
                if (base.TryGetValue(LATITUDE, out ret))
                    return Convert.ToDouble(ret);
                else
                    return 0.0;
            }
        }

        public double Longitude
        {
            get
            {
                object ret;
                if (base.TryGetValue(LONGITUDE, out ret) && ret is double)
                    return Convert.ToDouble(ret);
                else
                    return 0.0;
            }
        }

        private Graphic _graphic;
        public Graphic Graphic
        {
            get
            {
                if (_graphic != null)
                    return _graphic;
                if (UnGeocoded)
                    return null;
                _graphic = new Graphic(new MapPoint(Longitude, Latitude, SpatialReferences.Wgs84), this);
                return _graphic;
            }
        }

        private bool _assignedIncident;
        public bool AssignedIncident
        {
            get { return _assignedIncident; }
            set
            {
                _assignedIncident = value;
                NotifyPropertyChanged();
            }
        }
        
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                NotifyPropertyChanged();
            }
        }

        public bool UnGeocoded
        {
            get { return Math.Abs(Latitude) < 1.0 && Math.Abs(Longitude) < 1.0; }
        }

        public Brush BorderColor
        {
            get { return AssignedIncident ? (Brush)Application.Current.Resources["VPAssigned"] : (Brush)Application.Current.Resources["VPTeal"]; }
        }

        public Brush BackGroundColor
        {
            get { return UnGeocoded ? (Brush)Application.Current.Resources["VPUnGeocoded"] : (Brush)Application.Current.Resources["VPGray"]; }
        }

        public static String FirstDisplayItemKey { get; set; }
        public static String SecondDisplayItemKey { get; set; }
        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region public methods
        #endregion
        #endregion

        #region private
        public const String UNIQUE_ID = "Incident_ID";
        private const String UNIT_ID = "Unit_id";
        public const String CALL_TYPE = "Call_Type";
        private const String RECORD_TIME = "Rec_Time";
        public const String LATITUDE = "Lat";
        public const String LONGITUDE = "Lon";
        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
