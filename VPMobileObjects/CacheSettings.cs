using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VPMobileObjects
{    public enum CacheSyncTypes
    {
        AlwaysSync,
        AdminSync,
        NeverSync
    }

    [XmlSerializerFormat]
    public class CacheSettings : INotifyPropertyChanged
    {
        #region public
                #region public constructor
        public CacheSettings()
        {
            IdentifyingLayers = new List<String>();
            LastUpdate = DateTime.Now;
        }
        #endregion

        #region public properties

        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "URL")]
        public string URL { get; set; }

        [XmlAttribute(AttributeName = "IsBaseMap")]
        public bool IsBaseMap { get; set; }

        [XmlAttribute(AttributeName = "IsVisibleDefault")]
        public bool IsVisibleDefault { get; set; }

        [XmlAttribute(AttributeName = "SyncType")]
        public CacheSyncTypes SyncType { get; set; }

        private DateTime _lastUpdate;
        [XmlAttribute(AttributeName = "LastUpdate")]
        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
            set
            {
                _lastUpdate = value;
                NotifyPropertyChanged();
            }
        }

        [XmlAttribute(AttributeName = "PurgeOnSync")]
        public bool PurgeOnSync { get; set; }

        [XmlIgnore]
        public List<String> IdentifyingLayers { get; set; }
        [XmlElement(ElementName = "IdentifyingLayers")]
        public string XMLIdentifyingLayers
        {
            get
            {
                if (!IdentifyingLayers.Any())
                    return String.Empty;
                return IdentifyingLayers.Aggregate((aggregate, cur) => aggregate + "," + cur);
            }
            set
            {
                IdentifyingLayers = value.Split(',').ToList<String>();
            }
        }

        [XmlElement(ElementName = "SyncEnvelope")]
        public Envelope SyncEnvelope { get; set; }
        #endregion
        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #endregion

        #region private
        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
