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
    public class Envelope
    {
        #region public
        #region public properties
        [XmlAttribute(AttributeName = "XMax")]
        public double XMax { get; set; }

        [XmlAttribute(AttributeName = "XMin")]
        public double XMin { get; set; }

        [XmlAttribute(AttributeName = "YMax")]
        public double YMax { get; set; }

        [XmlAttribute(AttributeName = "YMin")]
        public double YMin { get; set; }

        [XmlAttribute(AttributeName = "WKID")]
        public int WKID { get; set; }

        [XmlAttribute(AttributeName = "MinScale")]
        public double MinScale { get; set; }

        [XmlAttribute(AttributeName = "MaxScale")]
        public double MaxScale { get; set; }
        #endregion
        #endregion
    }
}
