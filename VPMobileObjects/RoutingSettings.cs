using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VPMobileObjects
{
    [XmlSerializerFormat]
    public class RoutingSettings
    {
        #region public
        #region public properties
        [XmlElement(ElementName = "ShapeFilePath")]
        public string ShapeFilePath { get; set; }

        [XmlElement(ElementName = "StreetNameField")]
        public string StreetNameField { get; set; }

        [XmlElement(ElementName = "SpeedLimitField")]
        public string SpeedLimitField { get; set; }

        [XmlElement(ElementName = "OneWayField")]
        public string OneWayField { get; set; }

        [XmlElement(ElementName = "OneWayFieldIndicator")]
        public string OneWayFieldIndicator { get; set; }

        [XmlElement(ElementName = "WKID")]
        public int WKID { get; set; }
        #endregion
        #endregion
    }
}
