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
    public class StreetFinderSettings
    {
        #region public
        #region public properties
        [XmlAttribute(AttributeName = "Layer")]
        public string LayerName { get; set; }

        [XmlAttribute(AttributeName = "LayerID")]
        public int LayerID { get; set; }

        [XmlAttribute(AttributeName = "Service")]
        public string CacheName { get; set; }

        [XmlAttribute(AttributeName = "StreetField")]
        public string StreetNameField { get; set; }
        #endregion
        #endregion
    }
}
