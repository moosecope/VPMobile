using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VPMobileObjects
{
    public enum GeocoderTypes
    {
        [Description("Single Field")]
        SingleField,
        [Description("Single House")]
        SingleHouse//,
        //[Description("Single Range")]
        //SingleRange,
        //[Description("Dual Range")]
        //DualRange
    }

    [XmlSerializerFormat]
    public class GeocoderSettings : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public GeocoderSettings()
        {
            MinMatchScore = 75;
        }
        #endregion

        #region public properties
        private string _layerName;
        [XmlElement(ElementName = "Layer")]
        public string LayerName
		{
			get
			{
				return _layerName;
			}
			set
			{
				_layerName = value;
				NotifyPropertyChanged();
			}
		}

        private int _layerId;
        [XmlElement(ElementName = "LayerID")]
        public int LayerID
        {
            get { return _layerId; }
            set
            {
                _layerId = value;
                NotifyPropertyChanged();
            }
        }
        
        private string _cacheName;
        [XmlElement(ElementName = "Cache")]
        public string CacheName
		{
			get
			{
				return _cacheName;
			}
			set
			{
				_cacheName = value;
				NotifyPropertyChanged();
			}
		}

        private string _houseNumberField;
        [XmlElement(ElementName = "HouseNumberField")]
        public string HouseNumberField
		{
			get
			{
				return _houseNumberField;
			}
			set
			{
				_houseNumberField = value;
				NotifyPropertyChanged();
			}
		}

        private string _leftFromField;
        [XmlElement(ElementName = "LeftFromField")]
        public string LeftFromField
		{
			get
			{
				return _leftFromField;
			}
			set
			{
				_leftFromField = value;
				NotifyPropertyChanged();
			}
		}

        private string _leftToField;
        [XmlElement(ElementName = "LeftToField")]
        public string LeftToField
		{
			get
			{
				return _leftToField;
			}
			set
			{
				_leftToField = value;
				NotifyPropertyChanged();
			}
		}

        private string _rightFromField;
        [XmlElement(ElementName = "RightFromField")]
        public string RightFromField
		{
			get
			{
				return _rightFromField;
			}
			set
			{
				_rightFromField = value;
				NotifyPropertyChanged();
			}
		}

        private string _rightToField;
        [XmlElement(ElementName = "RightToField")]
        public string RightToField
		{
			get
			{
				return _rightToField;
			}
			set
			{
				_rightToField = value;
				NotifyPropertyChanged();
			}
		}

        private string _preDirField;
        [XmlElement(ElementName = "PreDirField")]
        public string PreDirField
		{
			get
			{
				return _preDirField;
			}
			set
			{
				_preDirField = value;
				NotifyPropertyChanged();
			}
		}

        private string _streetNameField;
        [XmlElement(ElementName = "StreetNameField")]
        public string StreetNameField
		{
			get
			{
				return _streetNameField;
			}
			set
			{
				_streetNameField = value;
				NotifyPropertyChanged();
			}
		}

        private string _streetTypeField;
        [XmlElement(ElementName = "StreetTypeField")]
        public string StreetTypeField
		{
			get
			{
				return _streetTypeField;
			}
			set
			{
				_streetTypeField = value;
				NotifyPropertyChanged();
			}
		}

        private string _sufDirField;
        [XmlElement(ElementName = "SufDirField")]
        public string SufDirField
		{
			get
			{
				return _sufDirField;
			}
			set
			{
				_sufDirField = value;
				NotifyPropertyChanged();
			}
		}

        private string _aptNumberField;
        [XmlElement(ElementName = "AptNumberField")]
        public string AptNumberField
		{
			get
			{
				return _aptNumberField;
			}
			set
			{
				_aptNumberField = value;
				NotifyPropertyChanged();
			}
		}

        private string _zoneField;
        [XmlElement(ElementName = "ZoneField")]
        public string ZoneField
		{
			get
			{
				return _zoneField;
			}
			set
			{
				_zoneField = value;
				NotifyPropertyChanged();
			}
		}

        private int _minMatchScore;
        [XmlElement(ElementName = "MinMatchScore")]
        public int MinMatchScore
		{
			get
			{
				return _minMatchScore;
			}
			set
			{
				_minMatchScore = value;
				NotifyPropertyChanged();
			}
		}

        private GeocoderTypes _type;
        [XmlElement(ElementName = "Type")]
        public GeocoderTypes Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
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
