using ArcGisServiceInfo.ServicesInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using VPMobileAdmin.StaticHelpers;
using VPMobileAdmin.Views;

namespace VPMobileAdmin.ViewModels
{
    public enum ServiceTypes
    {
        [Description("Tile Service")]
        tile,
        [Description("Feature Service")]
        feature
    }

    public class ArcgisServiceViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public ArcgisServiceViewModel()
        {
            Services = new ObservableCollection<ArcgisService>();
            if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.DefaultArcgisServer))
                ServerUrl = Properties.Settings.Default.DefaultArcgisServer;
        }
        #endregion

        #region public properties
        private String _serverUrl;
        public String ServerUrl
        {
            get { return _serverUrl; }
            set
            {
                _serverUrl = value;
                NotifyPropertyChanged();
                ArcGisServiceInfo.Catalog.FindServicesCompleted += FindServicesCompleted;
                ArcGisServiceInfo.Catalog.FindServices(_serverUrl);
            }
        }

        public IEnumerable<Tuple<Enum, String>> ServiceTypes => EnumHelper.GetAllValuesAndDescriptions<ServiceTypes>();

        private ServiceTypes _serviceTypeEnum;
        public ServiceTypes ServiceTypeEnum
        {
            get
            {
                return _serviceTypeEnum;
            }
            set
            {
                _serviceTypeEnum = value;
                NotifyPropertyChanged();
                ServicesView.Refresh();
            }
        }

        private bool _dropDownOpen;
        public bool IsDropDownOpen
        {
            get { return _dropDownOpen; }
            set
            {
                _dropDownOpen = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<ArcgisService> _services;
        public ObservableCollection<ArcgisService> Services
        {
            get { return _services; }
            set
            {
                _services = value;
                NotifyPropertyChanged();
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    _servicesView = CollectionViewSource.GetDefaultView(_services);
                    _servicesView.Filter = ServiceFilter;
                    NotifyPropertyChanged("ServicesView");
                });
            }
        }

        private ICollectionView _servicesView;
        public ICollectionView ServicesView
        {
            get { return _servicesView; }
        }

        private ArcgisService _service;
        public ArcgisService Service
        {
            get { return _service; }
            set
            {
                _service = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region public methods
        #endregion
        #endregion

        #region private
        private int _servicesLeft;

        //  This method is called by the Set accessor of each property.
        //  The CallerMemberName attribute that is applied to the optional propertyName
        //  parameter causes the property name of the caller to be substituted as an argument.
        //  Note: Requires Framework 4.5 or higher
        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool ServiceFilter(object item)
        {
            Service service = item as Service;
            switch(ServiceTypeEnum)
            {
                case ViewModels.ServiceTypes.tile:
                    return item is Service && service.SingleFusedMapCache;
                case ViewModels.ServiceTypes.feature:
                    return item is Feature;
                default:
                    return true;
            }
        }

        private void FindServicesCompleted(object sender, ArcGisServiceInfo.CatalogData.FindServicesEventArgs e)
        {
            ArcGisServiceInfo.Catalog.FindServicesCompleted -= FindServicesCompleted;
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling arcgis services from server", e.Error);
                    return;
                }
                ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfoCompleted += ServiceInfo_RetrieveServiceInfoCompleted;
                ArcGisServiceInfo.ServiceInfo.RetrieveFeatureInfoCompleted += ServiceInfo_RetrieveFeatureInfoCompleted;
                _servicesLeft = e.Services.Count();
                foreach (var service in e.Services)
                {
                    ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfo(service.Name);
                }
                Services = new ObservableCollection<ArcgisService>();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling arcgis services from server", ex);
            }
        }

        private void ServiceInfo_RetrieveFeatureInfoCompleted(object sender, RetrieveFeatureInfoEventArgs e)
        {
            if (--_servicesLeft <= 1)
            {
                ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfoCompleted -= ServiceInfo_RetrieveServiceInfoCompleted;
                ArcGisServiceInfo.ServiceInfo.RetrieveFeatureInfoCompleted -= ServiceInfo_RetrieveFeatureInfoCompleted;
                IsDropDownOpen = true;
            }
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling arcgis service from server", e.Error);
                    return;
                }

                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Services.Add(e.FeatureData);
                    NotifyPropertyChanged("ServicesView");
                });
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling arcgis services from server", ex);
            }
        }

        private void ServiceInfo_RetrieveServiceInfoCompleted(object sender, RetrieveServiceInfoEventArgs e)
        {
            if (--_servicesLeft <= 1)
            {
                ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfoCompleted -= ServiceInfo_RetrieveServiceInfoCompleted;
                ArcGisServiceInfo.ServiceInfo.RetrieveFeatureInfoCompleted -= ServiceInfo_RetrieveFeatureInfoCompleted;
                IsDropDownOpen = true;
            }
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling arcgis service from server", e.Error);
                    return;
                }
                
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Services.Add(e.ServiceData);
                    NotifyPropertyChanged("ServicesView");
                });
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling arcgis services from server", ex);
            }
        }
        #endregion
    }
}
