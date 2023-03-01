using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;



namespace VPMobileObjects
{
    [XmlSerializerFormat]
    public class VPMobileSettings : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public VPMobileSettings()
        {
            MapIconLarge = false;
            MapTextLarge = false;
            AvlServerAddress = null;
            ReportUnitNumber = false;
            AvlEnabled = true;
            LegendEnabled = false;
            AvlLocOption = false;
            FirstIncidentDisplayField = "Incident_ID";
            SecondIncidentDisplayField = "Address";
            StreetList = new ObservableCollection<StreetFinderSettings>();
            Geocoders = new ObservableCollection<GeocoderSettings>();
            MapServices = new ObservableCollection<CacheSettings>();
            Routing = new RoutingSettings();
            DispatchGroups = new List<String>();
            AvlGroups = new List<int>();
        }
        #endregion

        #region public properties
        private String _name;
        [XmlElement(ElementName = "Name")]
        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private String _description;
        [XmlElement(ElementName = "Description")]
        public String Description
        {
            get { return _description; }
            set
            {
                _description = value;
                NotifyPropertyChanged();
            }
        }

        private Envelope _fullExtent;
        [XmlElement(ElementName = "FullExtent")]
        public Envelope FullExtent
        {
            get { return _fullExtent; }
            set
            {
                _fullExtent = value;
                NotifyPropertyChanged();
            }
        }

        private bool _mapIconLarge;
        [XmlElement(ElementName = "MapIconLarge")]
        public bool MapIconLarge
        {
            get { return _mapIconLarge; }
            set
            {
                _mapIconLarge = value;
                NotifyPropertyChanged();
            }
        }

        [XmlIgnore]
        public int MapIconSize
        {
            get { return _mapIconLarge ? 42 : 21; }
        }

        private bool _mapTextLarge;
        [XmlElement(ElementName = "MapTextLarge")]
        public bool MapTextLarge
        {
            get { return _mapTextLarge; }
            set
            {
                _mapTextLarge = value;
                NotifyPropertyChanged();
            }
        }

        public int MapTextSize
        {
            get { return _mapTextLarge ? 18 : 12; }
        }

        private String _avlServerAddress;
        [XmlElement(ElementName = "AVLServer")]
        public String AvlServerAddress
        {
            get { return _avlServerAddress; }
            set
            {
                _avlServerAddress = value;
                NotifyPropertyChanged();
            }
        }

        private bool _avlListVisible;
        [XmlElement(ElementName = "AvlListVisible")]
        public bool AvlListVisible
        {
            get { return _avlListVisible; }
            set
            {
                _avlListVisible = value;
                NotifyPropertyChanged();
            }
        }

        private bool _reportUnitNumber;
        [XmlElement(ElementName = "ReportUnitNumber")]
        public bool ReportUnitNumber
        {
            get { return _reportUnitNumber; }
            set
            {
                _reportUnitNumber = value;
                NotifyPropertyChanged();
            }
        }

        private bool _avlEnabled;
        [XmlElement(ElementName = "AVLEnabled")]
        public bool AvlEnabled
        {
            get { return _avlEnabled; }
            set
            {
                _avlEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private bool _legendEnabled;
        [XmlElement(ElementName = "LegendEnabled")]
        public bool LegendEnabled
        {
            get { return _legendEnabled; }
            set
            {
                _legendEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private bool _avlLocOption;
        [XmlElement(ElementName = "ShowAVLLocationOption")]
        public bool AvlLocOption
        {
            get { return _avlLocOption; }
            set
            {
                _avlLocOption = value;
                NotifyPropertyChanged();
            }
        }

        private String _firstIncidentDisplayField;
        [XmlElement(ElementName = "FirstIncidentDisplayField")]
        public String FirstIncidentDisplayField
        {
            get { return _firstIncidentDisplayField; }
            set
            {
                _firstIncidentDisplayField = value;
                NotifyPropertyChanged();
            }
        }

        private String _secondIncidentDisplayField;
        [XmlElement(ElementName = "SecondIncidentDisplayField")]
        public String SecondIncidentDisplayField
        {
            get { return _secondIncidentDisplayField; }
            set
            {
                _secondIncidentDisplayField = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<StreetFinderSettings> _streetList;
        [XmlArray(ElementName = "StreetFinders"), XmlArrayItem(ElementName = "StreetFinder")]
        public ObservableCollection<StreetFinderSettings> StreetList
        {
            get { return _streetList; }
            set
            {
                _streetList = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<GeocoderSettings> _geocoders;
        [XmlArray(ElementName = "Geocoders"), XmlArrayItem(ElementName = "Geocoder")]
        public ObservableCollection<GeocoderSettings> Geocoders
        {
            get { return _geocoders; }
            set
            {
                _geocoders = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<CacheSettings> _mapLayers;
        [XmlArray(ElementName = "Caches"), XmlArrayItem(ElementName = "Cache")]
        public ObservableCollection<CacheSettings> MapServices
        {
            get { return _mapLayers; }
            set
            {
                _mapLayers = value;
                NotifyPropertyChanged();
            }
        }

        private RoutingSettings _routing;
        [XmlElement(ElementName = "RoutingSettings")]
        public RoutingSettings Routing
        {
            get { return _routing; }
            set
            {
                _routing = value;
                NotifyPropertyChanged();
            }
        }

        private Guid _dispatchID;
        [XmlElement(ElementName = "DispatchID")]
        public Guid DispatchID
        {
            get { return _dispatchID; }
            set
            {
                _dispatchID = value;
                NotifyPropertyChanged();
            }
        }

        private Guid _dispatchConfigID;
        [XmlElement(ElementName = "DispatchConfigID")]
        public Guid DispatchConfigID
        {
            get { return _dispatchConfigID; }
            set
            {
                _dispatchConfigID = value;
                NotifyPropertyChanged();
            }
        }

        private List<String> _dispatchGroups;
        [XmlArray(ElementName = "DispatchGroups"), XmlArrayItem(ElementName = "string")]
        public List<String> DispatchGroups
        {
            get { return _dispatchGroups; }
            set
            {
                if (value != null)
                {
                    _dispatchGroups = value.Distinct().ToList();
                }
                NotifyPropertyChanged();
            }
        }

        private Guid _avlID;
        [XmlElement(ElementName = "AvlID")]
        public Guid AvlID
        {
            get { return _avlID; }
            set
            {
                _avlID = value;
                NotifyPropertyChanged();
            }
        }

        private Guid _avlConfigID;
        [XmlElement(ElementName = "AvlConfigID")]
        public Guid AvlConfigID
        {
            get { return _avlConfigID; }
            set
            {
                _avlConfigID = value;
                NotifyPropertyChanged();
            }
        }

        private List<int> _avlGroups;
        [XmlArray(ElementName = "AVLGroups"), XmlArrayItem(ElementName = "int")]
        public List<int> AvlGroups
        {
            get { return _avlGroups; }
            set
            {
                _avlGroups = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        #endregion

        #region private
        //  This method is called by the Set accessor of each property.
        //  The CallerMemberName attribute that is applied to the optional propertyName
        //  parameter causes the property name of the caller to be substituted as an argument.
        //  Note: Requires Framework 4.5 or higher
        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}