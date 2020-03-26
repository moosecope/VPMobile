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
    public class UserSettings
    {
        #region public
        #region public constructor
        public UserSettings()
        {
            IncidentZoomWidth = 2000;
            IncidentRefreshInterval = 10000;
            GpsKeepNorth = false;
            AvlLocLabel = false;
            PoliceVisible = true;
            FireVisible = true;
            EmsVisible = true;
            OtherVisible = true;
            DynamicZoom = false;
        }
        #endregion

        #region public properties
        [XmlElement(ElementName = "UnitNumber")]
        public string UnitNumber { get; set; }

        [XmlElement(ElementName = "IncidentZoomWidth")]
        public int IncidentZoomWidth { get; set; }

        [XmlElement(ElementName = "IncidentRefreshInterval")]
        public int IncidentRefreshInterval { get; set; }

        [XmlElement(ElementName = "GPSKeepCentered")]
        public bool GpsKeepCentered { get; set; }

        [XmlElement(ElementName = "GPSKeepNorth")]
        public bool GpsKeepNorth { get; set; }

        [XmlElement(ElementName = "AutoRoute")]
        public bool AutoRoute { get; set; }

        [XmlElement(ElementName = "IncidentLabelVisible")]
        public bool IncidentLabel { get; set; }

        [XmlElement(ElementName = "AvlLabelVisible")]
        public bool AvlLabel { get; set; }

        [XmlElement(ElementName = "AvlLocationLabelVisible")]
        public bool AvlLocLabel { get; set; }

        [XmlElement(ElementName = "GPSLocationLabelVisible")]
        public bool GpsLocLabel { get; set; }

        [XmlElement(ElementName = "PoliceIncidentsVisible")]
        public bool PoliceVisible { get; set; }

        [XmlElement(ElementName = "FireIncidentsVisible")]
        public bool FireVisible { get; set; }

        [XmlElement(ElementName = "EmsIncidentsVisible")]
        public bool EmsVisible { get; set; }

        [XmlElement(ElementName = "OtherIncidentsVisible")]
        public bool OtherVisible { get; set; }

        [XmlElement(ElementName = "DynamicZoom")]
        public bool DynamicZoom { get; set; }
        #endregion
        #endregion

    }
}
