using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace VPMobileObjects
{
    #region public enum
    public enum GpsType
    {
        None,
        COM,
        UDP
    }
    #endregion
    public class GpsSettings
    {
        #region public

        #region public constructor
        public GpsSettings()
        {
            Type = GpsType.None;
            Port = "COM1";
            ComBaudRate = 4800;
            IsSungard = false;
        }
        #endregion

        #region public properties
        [XmlElement( ElementName = "Type" )]
		public GpsType Type { get; set; }
        [XmlElement( ElementName = "Port" )]
		public string Port { get; set; }
        [XmlElement( ElementName = "Baud_Rate" )]
		public int ComBaudRate { get; set; }
        [XmlElement(ElementName = "Is_Sungard")]
        public bool IsSungard { get; set; }
        #endregion
        #endregion
    }
}
