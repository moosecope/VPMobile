using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VPMobileAdmin.StaticHelpers;
using VPMobileAdmin.VPMobileService;
using VPMobileAdmin.VPCoreService;
using VPMobileObjects;

using log4net;

using UTIL = GTG.Utilities;
using VPMobileAdmin.Views;
using System.Windows;
using System.Security.Principal;
using VPMobileAdmin.Models;
using ArcGisServiceInfo.ServicesInfo;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace VPMobileAdmin.ViewModels
{
    public class EditConfigurationViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor

        public EditConfigurationViewModel()
        {
            _mobileService = new VPMobileServiceClient();
            _mobileService.GetAllRoutingFileInfoCompleted += GetAllRoutingFileInfoCompleted;

            PropertyChanged += OnPropertyChanged;
            _configuration = new VPMobileSettings();
            _dispatchSettings = new ObservableCollection<VPMobileDispatchSettings>();
            _visibleDispatchGroups = new ObservableCollection<Pair<bool, VPDispatchIncidentTypeSettings>>();
            _avlSettings = new ObservableCollection<VPMobileAVLSettings>();
            _visibleAvlGroups = new ObservableCollection<Pair<bool, VPAVLGroupInfo>>();
            _routingFiles = new ObservableCollection<RoutingFileInfo>();
            _mobileService.UpdateConfigCompleted += UpdateConfigCompleted;
            _mobileService.AddConfigCompleted += AddConfigCompleted;
            _vpmService = new VPMServiceClient();

            GetDispatchSettings();
            GetAVLSettings();

            //_vpService.GetAvailableAvlSettingsCompleted += GetAvailableAvlSettingsCompleted;
            //_vpmService.GetAvailableDispatchSettingsCompleted += GetAvailableDispatchSettingsCompleted;
        }
        #endregion

        #region public properties

        private VPMobileSettings _configuration;
        public VPMobileSettings Configuration
        {
            get { return _configuration; }
            set
            {
                if (_configuration != null)
                    _configuration.PropertyChanged -= PropertyChanged;
                _configuration = value;
                if (_configuration != null)
                {
                    _configuration.PropertyChanged += PropertyChanged;
                    _newConfig = String.IsNullOrWhiteSpace(_configuration.Name);
                }
                RoutingFiles = new ObservableCollection<RoutingFileInfo>(_mobileService.GetAllRoutingFileInfo(Guid.NewGuid().ToString()));
                GetDispatchSettings();
                GetAVLSettings();

                if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.AvlServerAddress))
                {
                    _configuration.AvlServerAddress = Properties.Settings.Default.AvlServerAddress;
                }
                NotifyPropertyChanged();
                _propertiesChanged = 0;
            }
        }

        private EditConfigurationWindow _view;
        public EditConfigurationWindow View
        {
            get { return _view; }
            set
            {
                _view = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<VPMobileDispatchSettings> _dispatchSettings;
        public ObservableCollection<VPMobileDispatchSettings> DispatchSettings
        {
            get { return _dispatchSettings; }
            set
            {
                _dispatchSettings = value;
                NotifyPropertyChanged();
                if (Configuration == null)
                    return;
                var selected = DispatchSettings.FirstOrDefault(grp => grp.DispatchID == Configuration.DispatchID || grp.ConfigID == Configuration.DispatchConfigID);
                if (selected != null)
                    SelectedDispatchSetting = selected;
            }
        }

        public VPMobileDispatchSettings SelectedDispatchSetting
        {
            get
            {
                return DispatchSettings.FirstOrDefault(grp => grp.DispatchID == Configuration.DispatchID || grp.ConfigID == Configuration.DispatchConfigID);
            }
            set
            {
                Configuration.DispatchID = value.DispatchID;
                Configuration.DispatchConfigID = value.ConfigID;
                VisibleDispatchGroups = new ObservableCollection<Pair<bool, VPDispatchIncidentTypeSettings>>(value.IncidentTypes.Select(grp => new Pair<bool, VPDispatchIncidentTypeSettings>(Configuration.DispatchGroups.Contains(grp.IncidentType), grp)));
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Pair<bool, VPDispatchIncidentTypeSettings>> _visibleDispatchGroups;
        public ObservableCollection<Pair<bool, VPDispatchIncidentTypeSettings>> VisibleDispatchGroups
        {
            get
            {
                return _visibleDispatchGroups;
            }
            set
            {
                _visibleDispatchGroups = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<VPMobileAVLSettings> _avlSettings;
        public ObservableCollection<VPMobileAVLSettings> AvlSettings
        {
            get { return _avlSettings; }
            set
            {
                _avlSettings = value;
                NotifyPropertyChanged();
                if (Configuration == null)
                    return;
                var selected = AvlSettings.FirstOrDefault(grp => grp.AVLID == Configuration.AvlID || grp.ConfigID == Configuration.AvlConfigID);
                if (selected != null)
                    SelectedAvlSetting = selected;
            }
        }

        public VPMobileAVLSettings SelectedAvlSetting
        {
            get
            {
                return AvlSettings.FirstOrDefault(grp => grp.AVLID == Configuration.AvlID || grp.ConfigID == Configuration.AvlConfigID);
            }
            set
            {
                Configuration.AvlID = value.AVLID;
                Configuration.AvlConfigID = value.ConfigID;
                if (value == null)
                    MessageBox.Show("Selected Avl Setting is null");
                if(value.Groups == null)
                    MessageBox.Show("Selected Avl Setting Groups is null");
                if (Configuration.AvlGroups == null)
                    MessageBox.Show("Configuration Avl Groups is null");

                VisibleAvlGroups = new ObservableCollection<Pair<bool, VPAVLGroupInfo>>(value.Groups.Select(grp => new Pair<bool, VPAVLGroupInfo>(Configuration.AvlGroups.Contains(grp.GroupID), grp)));
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<Pair<bool, VPAVLGroupInfo>> _visibleAvlGroups;
        public ObservableCollection<Pair<bool, VPAVLGroupInfo>> VisibleAvlGroups
        {
            get
            {
                return _visibleAvlGroups;
            }
            set
            {
                _visibleAvlGroups = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<RoutingFileInfo> _routingFiles;
        public ObservableCollection<RoutingFileInfo> RoutingFiles
        {
            get { return _routingFiles; }
            set
            {
                _routingFiles = value;                
                NotifyPropertyChanged();
            }
        }

        public RoutingFileInfo SelectedRoutingFile
        {
            get
            {
                return RoutingFiles.FirstOrDefault(rf => rf.RoutingFileName == Configuration.Routing.ShapeFilePath) ?? new RoutingFileInfo();
            }
            set
            {
                Configuration.Routing.ShapeFilePath = value.RoutingFileName;
                NotifyPropertyChanged();
            }
        }        
        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public methods

        #region General Tab
        public void AddCacheSettings()
        {
            try
            {
                var dlgService = new ArcgisServiceDialog();
                dlgService.Owner = View;
                dlgService.ShowDialog();

                if (dlgService.DialogResult == true)
                {
                    var service = dlgService.ViewModel.Service;

                    if (service == null)
                    {
                        string message = String.Format("Unable to verify the service.  Do you want to use the following serivce anyway?\n\n{0}", dlgService.ViewModel.ServerUrl);
                        MessageBoxResult rslt = MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo);
                        if (rslt == MessageBoxResult.Yes)
                        {
                            var dlgCache = new EditCacheDialog();
                            dlgCache.Owner = View;
                            dlgCache.ViewModel.AddMapServiceByUrl(dlgService.ViewModel.ServerUrl);
                            dlgCache.ShowDialog();

                            if (dlgCache.DialogResult == true)
                            {
                                Configuration.MapServices.Add(dlgCache.ViewModel.Configuration);
                            }
                        }

                    }
                    else
                    {
                        var dlgCache = new EditCacheDialog();
                        dlgCache.Owner = View;
                        dlgCache.ViewModel.MapService = service;
                        dlgCache.ShowDialog();

                        if (dlgCache.DialogResult == true)
                        {
                            Configuration.MapServices.Add(dlgCache.ViewModel.Configuration);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error adding cache settings", ex);
            }
        }

        public void EditCacheSettings(int index)
        {
            try
            {
                if (index < 0 || index >= Configuration.MapServices.Count)
                    return;

                _pollObject = Configuration.MapServices[index];

                ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfoCompleted += ServiceInfo_RetrieveServiceInfoCompleted;
                ArcGisServiceInfo.ServiceInfo.RetrieveFeatureInfoCompleted += ServiceInfo_RetrieveFeatureInfoCompleted;

                ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfo(Configuration.MapServices[index].URL);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling service info", ex);
            }
        }

        public void RemoveCacheSettings(int index)
        {
            try
            {
                if (index < 0 || index >= Configuration.MapServices.Count)
                    return;

                var srvc = Configuration.MapServices[index];
                var gcdrUsed = Configuration.Geocoders.Where(gcdr => gcdr.CacheName == srvc.Name).ToList();
                var stintUsed = Configuration.StreetList.Where(stint => stint.CacheName == srvc.Name).ToList();
                if ((gcdrUsed.Count() > 0 || stintUsed.Count() > 0) && MessageBox.Show("This cache is associated with a geocoder or street intersection finder.\r\n Are you sure you want to delete this cache and associated items?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
                Configuration.MapServices.RemoveAt(index);
                foreach (var gcdr in gcdrUsed)
                {
                    Configuration.Geocoders.Remove(gcdr);
                }
                foreach (var stint in stintUsed)
                {
                    Configuration.StreetList.Remove(stint);
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error removing cache settings", ex);
            }
        }

        public void MoveCacheSettingsUp(int index)
        {
            try
            {
                if (index < 1 || index >= Configuration.MapServices.Count)
                    return;

                Configuration.MapServices.Move(index, index - 1);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error moving cache settings up", ex);
            }
        }

        public void MoveCacheSettingsDown(int index)
        {
            try
            {
                if (index < 0 || index >= Configuration.MapServices.Count - 1)
                    return;

                Configuration.MapServices.Move(index, index + 1);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error moving cache settings down", ex);
            }
        }
        #endregion

        #region Location Tools Tab
        public void AddGeocoder()
        {
            try
            {
                var dlgLayer = new SelectCacheLayerDialog();
                dlgLayer.Owner = View;
                dlgLayer.ViewModel.Caches = Configuration.MapServices.Where(srvc => !srvc.IsBaseMap);
                dlgLayer.ShowDialog();

                if (dlgLayer.DialogResult == true)
                {
                    var cache = dlgLayer.ViewModel.Caches.FirstOrDefault(cach => cach.URL == dlgLayer.ViewModel.ServiceUrl);
                    var layer = dlgLayer.ViewModel.Layer;

                    var dlgCache = new EditGeocoderDialog();
                    dlgCache.Owner = View;
                    dlgCache.ViewModel.LayerInfo = layer;
                    dlgCache.ViewModel.Configuration.CacheName = cache.Name;
                    dlgCache.ViewModel.Configuration.LayerName = layer.Name;
                    dlgCache.ViewModel.Configuration.LayerID = layer.ID;
                    dlgCache.ShowDialog();

                    if (dlgCache.DialogResult == true)
                    {
                        Configuration.Geocoders.Add(dlgCache.ViewModel.Configuration);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error adding cache settings", ex);
            }
        }

        public void EditGeocoder(int index)
        {
            try
            {
                if (index < 0 || index >= Configuration.Geocoders.Count)
                    return;

                _pollObject = Configuration.Geocoders[index];
                ArcGisServiceInfo.ServiceInfo.RetrieveFeatureInfoCompleted += ServiceInfo_RetrieveFeatureInfoCompleted;
                ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfo(Configuration.MapServices.FirstOrDefault(srvc => srvc.Name == Configuration.Geocoders[index].CacheName)?.URL);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling service info", ex);
            }
        }

        public void RemoveGeocoder(int index)
        {
            try
            {
                if (index < 0 || index >= Configuration.Geocoders.Count)
                    return;

                Configuration.Geocoders.RemoveAt(index);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error removing cache settings", ex);
            }
        }
        public void AddStreetIntersectionFinder()
        {
            try
            {
                var dlgLayer = new SelectCacheLayerDialog();
                dlgLayer.Owner = View;
                dlgLayer.ViewModel.Caches = Configuration.MapServices.Where(srvc => !srvc.IsBaseMap);
                dlgLayer.ShowDialog();

                if (dlgLayer.DialogResult == true)
                {
                    var cache = dlgLayer.ViewModel.Caches.FirstOrDefault(cach => cach.URL == dlgLayer.ViewModel.ServiceUrl);
                    var layer = dlgLayer.ViewModel.Layer;

                    var dlgField = new SelectFieldDialog();
                    dlgField.Owner = View;
                    dlgField.ViewModel.Layer = layer;
                    dlgField.ShowDialog();

                    if (dlgField.DialogResult == true)
                    {
                        Configuration.StreetList.Add(new StreetFinderSettings
                        {
                            CacheName = cache.Name,
                            LayerName = layer.Name,
                            LayerID = layer.ID,
                            StreetNameField = dlgField.ViewModel.Field.Name
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error adding cache settings", ex);
            }
        }

        public void RemoveStreetIntersectionFinder(int index)
        {
            try
            {
                if (index < 0 || index >= Configuration.StreetList.Count)
                    return;
                
                Configuration.StreetList.RemoveAt(index);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error removing cache settings", ex);
            }
        }
        #endregion

        public void SaveConfiguration()
        {
            try
            {
                if(Configuration.DispatchID != default(Guid))
                {
                    Configuration.DispatchGroups = VisibleDispatchGroups.Where(grp => grp.First).Select(grp => grp.Second.IncidentType).ToList();
                }
                if(Configuration.AvlID != default(Guid))
                {
                    Configuration.AvlGroups = VisibleAvlGroups.Where(grp => grp.First).Select(grp => grp.Second.GroupID).ToList();
                }
                if (_newConfig)
                    _mobileService.AddConfigAsync(Guid.NewGuid().ToString(), Configuration);
                else
                    _mobileService.UpdateConfigAsync(Guid.NewGuid().ToString(), Configuration);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error while saving configuration", ex);
            }
        }

        public bool OnClosing()
        {
            try
            {
                if (_propertiesChanged > 0)
                {
                    switch (ErrorHelper.OnMessage("Save changes to this version before closing?", "Save and Exit", MessageBoxButton.YesNoCancel))
                    {
                        case MessageBoxResult.Cancel:
                            return true;
                        case MessageBoxResult.Yes:
                            SaveConfiguration();
                            if (_propertiesChanged > 0)
                                return true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error while closing", ex);
            }
            return false;
        }
        #endregion
        #endregion

        #region private
        private VPMobileServiceClient _mobileService;
        private VPMServiceClient _vpmService;
        private object _pollObject;
        private int _propertiesChanged;
        private bool _newConfig;
        //  This method is called by the Set accessor of each property.
        //  The CallerMemberName attribute that is applied to the optional propertyName
        //  parameter causes the property name of the caller to be substituted as an argument.
        //  Note: Requires Framework 4.5 or higher

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(object Sender, PropertyChangedEventArgs e)
        {
            _propertiesChanged++;
        }

        private void AddConfigCompleted(object sender, AddConfigCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error ", e.Error);
                    return;
                }
                if(e.Result)
                {
                    _newConfig = false;
                    _propertiesChanged = 0;
                    ErrorHelper.OnMessage("Config saved successfully");
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error adding config", ex);
            }
        }

        private void UpdateConfigCompleted(object sender, UpdateConfigCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error updating config settings", e.Error);
                    return;
                }
                if (e.Result)
                {
                    _propertiesChanged = 0;
                    ErrorHelper.OnMessage("Config saved successfully");
                }
                else
                {
                    ErrorHelper.OnMessage("Config was not able to be saved.\n" + e.ToString());
                }

            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error updating config settings", ex);
            }
        }

        private void ServiceInfo_RetrieveServiceInfoCompleted(object sender, RetrieveServiceInfoEventArgs e)
        {
            try
            {
                ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfoCompleted -= ServiceInfo_RetrieveServiceInfoCompleted;
                ArcGisServiceInfo.ServiceInfo.RetrieveFeatureInfoCompleted -= ServiceInfo_RetrieveFeatureInfoCompleted;
                if (e.Error != null)
                {
                    //ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling service info", e.Error);
                    string message = String.Format("Error pulling service info.\n\n{0}\n\nDo you want to edit the service anyway?", e.Error);
                    MessageBoxResult result = MessageBox.Show(message, "Edit Service", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                if (_pollObject is CacheSettings)
                {
                    var cache = _pollObject as CacheSettings;
                    var index = Configuration.MapServices.IndexOf(cache);
                    var name = cache.Name;
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var dlgCache = new EditCacheDialog();
                        dlgCache.Owner = View;
                        dlgCache.ViewModel.Configuration = cache;
                        dlgCache.ViewModel.MapService = e.ServiceData;
                        dlgCache.ShowDialog();

                        if (dlgCache.DialogResult == true)
                        {
                            Configuration.MapServices[index] = dlgCache.ViewModel.Configuration;
                            foreach (var gcdr in Configuration.Geocoders.Where(gcdr => gcdr.CacheName == name))
                            {
                                gcdr.CacheName = Configuration.MapServices[index].Name;
                            }
                            foreach (var stint in Configuration.StreetList.Where(stint => stint.CacheName == name))
                            {
                                stint.CacheName = Configuration.MapServices[index].Name;
                            }
                        }
                    }));
                }
                else if (_pollObject is GeocoderSettings)
                {
                    var geocoder = _pollObject as GeocoderSettings;
                    var index = Configuration.Geocoders.IndexOf(geocoder);
                    var cache = Configuration.MapServices.FirstOrDefault(cach => cach.Name == geocoder.CacheName);
                    var layer = e.ServiceData.Layers.FirstOrDefault(lyr => lyr.Name == geocoder.LayerName);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var dlgCache = new EditGeocoderDialog();
                        dlgCache.ViewModel.LayerInfo = layer;
                        dlgCache.ViewModel.Configuration = geocoder;
                        dlgCache.ShowDialog();

                        if (dlgCache.DialogResult == true)
                        {
                            Configuration.Geocoders[index] = dlgCache.ViewModel.Configuration;
                        }
                    }));
                }
                else
                {
                    ErrorHelper.OnMessage("Service polled for unknown object");
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error editing cache settings", ex);
            }
        }

        private void ServiceInfo_RetrieveFeatureInfoCompleted(object sender, RetrieveFeatureInfoEventArgs e)
        {
            try
            {
                ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfoCompleted -= ServiceInfo_RetrieveServiceInfoCompleted;
                ArcGisServiceInfo.ServiceInfo.RetrieveFeatureInfoCompleted -= ServiceInfo_RetrieveFeatureInfoCompleted;

                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling feature service info", e.Error);
                    return;
                }

                if (_pollObject is CacheSettings)
                {
                    var cache = _pollObject as CacheSettings;
                    var index = Configuration.MapServices.IndexOf(cache);
                    var name = cache.Name;
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var dlgCache = new EditCacheDialog();
                        dlgCache.Owner = View;
                        dlgCache.ViewModel.Configuration = cache;
                        dlgCache.ViewModel.MapService = e.FeatureData;
                        dlgCache.ShowDialog();

                        if (dlgCache.DialogResult == true)
                        {
                            Configuration.MapServices[index] = dlgCache.ViewModel.Configuration;
                            foreach (var gcdr in Configuration.Geocoders.Where(gcdr => gcdr.CacheName == name))
                            {
                                gcdr.CacheName = Configuration.MapServices[index].Name;
                            }
                            foreach (var stint in Configuration.StreetList.Where(stint => stint.CacheName == name))
                            {
                                stint.CacheName = Configuration.MapServices[index].Name;
                            }
                        }
                    }));
                }
                else if (_pollObject is GeocoderSettings)
                {
                    var geocoder = _pollObject as GeocoderSettings;
                    var index = Configuration.Geocoders.IndexOf(geocoder);
                    var cache = Configuration.MapServices.FirstOrDefault(cach => cach.Name == geocoder.CacheName);
                    var layer = e.FeatureData.Layers.FirstOrDefault(lyr => lyr.Name == geocoder.LayerName);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        var dlgCache = new EditGeocoderDialog();
                        dlgCache.ViewModel.LayerInfo = layer;
                        dlgCache.ViewModel.Configuration = geocoder;
                        dlgCache.ShowDialog();

                        if (dlgCache.DialogResult == true)
                        {
                            Configuration.Geocoders[index] = dlgCache.ViewModel.Configuration;
                        }
                    }));
                }
                else
                {
                    ErrorHelper.OnMessage("Service polled for unknown object");
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error editing cache settings", ex);
            }
        }

        #region Routing Tab
        private void GetAllRoutingFileInfoCompleted(object sender, GetAllRoutingFileInfoCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling routing settings", e.Error);
                    return;
                }
                RoutingFiles = new ObservableCollection<RoutingFileInfo>(e.Result);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling routing settings", ex);
            }
        }
        #endregion

        #region Dispatch Tab
        private void GetDispatchSettings()
        {
            string serviceURLBase = _vpmService.Endpoint.Address.Uri.AbsoluteUri;
            if (!serviceURLBase.EndsWith("/"))
            {
                serviceURLBase += "/";
            }
            string url = String.Format("{0}GetAvailableDispatchSettings", serviceURLBase);
            var address = new Uri(url);

            HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(address);
            request.Method = "GET";
            request.ContentType = "application/json";

            System.Net.HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Encoding encoder = Encoding.UTF8;
            StreamReader reader = new StreamReader(response.GetResponseStream(), encoder);
            string OutputData = reader.ReadToEnd();

            //VPMobileDispatchSettings
            DispatchSettingsRootobject jsonData = (DispatchSettingsRootobject)JsonConvert.DeserializeObject(OutputData, typeof(DispatchSettingsRootobject));
            DispatchSettings = new ObservableCollection<VPMobileDispatchSettings>(jsonData.GetAvailableDispatchSettingsResult);
        }

        /*
        private void GetAvailableDispatchSettingsCompleted(object sender, GetAvailableDispatchSettingsCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling available dispatch settings", e.Error);
                    return;
                }
                DispatchSettings = new ObservableCollection<VPMobileDispatchSettings>(e.Result);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling available dispatch settings", ex);
            }
        }

        private void GetAvailableAvlSettingsCompleted(object sender, GetAvailableAvlSettingsCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling available avl settings", e.Error);
                    return;
                }
                AvlSettings = new ObservableCollection<VPMobileAVLSettings>(e.Result);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling available avl settings", ex);
            }
        }
        */

        private void GetAVLSettings()
        {
            string serviceURLBase = _vpmService.Endpoint.Address.Uri.AbsoluteUri;
            if (!serviceURLBase.EndsWith("/"))
            {
                serviceURLBase += "/";
            }
            string url = String.Format("{0}GetAvailableAvlSettings", serviceURLBase);
            var address = new Uri(url);

            HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(address);
            request.Method = "GET";
            request.ContentType = "application/json";

            System.Net.HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Encoding encoder = Encoding.UTF8;
            StreamReader reader = new StreamReader(response.GetResponseStream(), encoder);
            string OutputData = reader.ReadToEnd();

            //VPMobileDispatchSettings
            AvlSettingsRootobject jsonData = (AvlSettingsRootobject)JsonConvert.DeserializeObject(OutputData, typeof(AvlSettingsRootobject));
            AvlSettings = new ObservableCollection<VPMobileAVLSettings>(jsonData.GetAvailableAvlSettingsResult);
        }

        #endregion
        #endregion

    }
}
