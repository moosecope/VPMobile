using ArcGisServiceInfo.ServicesInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VPMobileAdmin.StaticHelpers;
using VPMobileObjects;

namespace VPMobileAdmin.ViewModels
{
    public class EditGeocoderViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public EditGeocoderViewModel()
        {
            _configuration = new GeocoderSettings();
        }
        #endregion

        #region public properties
        private GeocoderSettings _configuration;
        public GeocoderSettings Configuration
        {
            get { return _configuration; }
            set
            {
                _configuration = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(GeocoderType));
            }
        }

        public IEnumerable<Tuple<Enum, String>> GeocoderTypes => EnumHelper.GetAllValuesAndDescriptions<GeocoderTypes>();
        
        public GeocoderTypes GeocoderType
        {
            get
            {
                return Configuration.Type;
            }
            set
            {
                Configuration.Type = value;
                NotifyPropertyChanged(nameof(HouseNumberEnabled));
                NotifyPropertyChanged(nameof(OthersEnabled));
                NotifyPropertyChanged(nameof(LeftEnabled));
                NotifyPropertyChanged(nameof(RightEnabled));
            }
        }

        public bool HouseNumberEnabled
        {
            get
            {
                return Configuration.Type == VPMobileObjects.GeocoderTypes.SingleHouse;
            }
        }

        public bool OthersEnabled
        {
            get
            {
                return Configuration.Type != VPMobileObjects.GeocoderTypes.SingleField;
            }
        }

        public bool LeftEnabled
        {
            get
            {
                return false; // Configuration.Type == VPMobileObjects.GeocoderTypes.SingleRange || Configuration.Type == VPMobileObjects.GeocoderTypes.DualRange;
            }
        }

        public bool RightEnabled
        {
            get
            {
                return false; // Configuration.Type == VPMobileObjects.GeocoderTypes.DualRange;
            }
        }

        private Layer _layerInfo;
        public Layer LayerInfo
        {
            get { return _layerInfo; }
            set
            {
                _layerInfo = value;
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
