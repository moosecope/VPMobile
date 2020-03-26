using ArcGisServiceInfo.ServicesInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VPMobileAdmin.Models;
using VPMobileObjects;

namespace VPMobileAdmin.ViewModels
{
    public class EditCacheViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public EditCacheViewModel()
        {
            _identifyingLayers = new ObservableCollection<Pair<bool, Layer>>();
            Configuration = new CacheSettings();
        }
        #endregion

        #region public properties

        private CacheSettings _configuration;
        public CacheSettings Configuration
        {
            get { return _configuration; }
            set
            {
                if (_configuration != null)
                    _configuration.PropertyChanged -= PropertyChanged;
                _configuration = value;
                if (_configuration != null)
                    _configuration.PropertyChanged += PropertyChanged;
                NotifyPropertyChanged();
            }
        }

        private ArcgisService _mapService;
        public ArcgisService MapService
        {
            get { return _mapService; }
            set
            {
                _mapService = value;
                NotifyPropertyChanged();
                var match = Regex.Match(_mapService.ServiceUrl, @"([^\/]*)\/[^\/]*Server");
                if (match.Success)
                {
                    Configuration.Name = match.Groups[1].Value;
                }
                Configuration.IsBaseMap = _mapService is Service;
                if(!Configuration.IsBaseMap)
                {
                    foreach (var lyr in ((Feature)_mapService).Layers)
                    {
                        _identifyingLayers.Add(new Pair<bool, Layer>(Configuration.IdentifyingLayers.Contains(lyr.Name), lyr));
                    }
                    NotifyPropertyChanged("IdentifyingLayers");
                }
            }
        }

        private ObservableCollection<Pair<bool, Layer>> _identifyingLayers;
        public ObservableCollection<Pair<bool, Layer>> IdentifyingLayers
        {
            get { return _identifyingLayers; }
        }
        
        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region public methods
        public void Update_Cache()
        {
            Configuration.LastUpdate = DateTime.Now;
        }

        public void Save()
        {
            Configuration.IdentifyingLayers = new List<string>(_identifyingLayers.Where(lyr => lyr.First).Select(lyr => lyr.Second.Name));
            Configuration.URL = MapService.ServiceUrl;
            Configuration.IsBaseMap = MapService is Service;
            if (Configuration.SyncEnvelope != null)
                Configuration.SyncEnvelope = null;
        }
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
