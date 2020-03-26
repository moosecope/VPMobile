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
    public class PointOfInterestSettings
    {
        #region public
        #region public properties
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "Lat")]
        public double Lat { get; set; }

        [XmlAttribute(AttributeName = "Long")]
        public double Long { get; set; }
        #endregion
        #endregion
    }
}
