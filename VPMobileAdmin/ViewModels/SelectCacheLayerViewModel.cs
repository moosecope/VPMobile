using ArcGisServiceInfo.ServicesInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VPMobileAdmin.StaticHelpers;
using VPMobileObjects;

namespace VPMobileAdmin.ViewModels
{
    public class SelectCacheLayerViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public SelectCacheLayerViewModel()
        {
        }
        #endregion

        #region public properties
        private IEnumerable<CacheSettings> _caches;
        public IEnumerable<CacheSettings> Caches
        {
            get { return _caches; }
            set
            {
                _caches = value;
                NotifyPropertyChanged();
            }
        }

        private String _serviceUrl;
        public String ServiceUrl
        {
            get { return _serviceUrl; }
            set
            {
                _serviceUrl = value;
                NotifyPropertyChanged();
                ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfoCompleted += RetrieveServiceInfoCompleted;
                ArcGisServiceInfo.ServiceInfo.RetrieveFeatureInfoCompleted += RetrieveFeatureInfoCompleted;
                ArcGisServiceInfo.ServiceInfo.RetrieveServiceInfo(_serviceUrl);
            }
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

        private Layer _layer;
        public Layer Layer
        {
            get { return _layer; }
            set
            {
                _layer = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isLayerDropDownOpen;
        public bool IsLayerDropDownOpen
        {
            get { return _isLayerDropDownOpen; }
            set
            {
                _isLayerDropDownOpen = value;
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

        private void RetrieveFeatureInfoCompleted(object sender, RetrieveFeatureInfoEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling feature service info", e.Error);
                    return;
                }
                Service = e.FeatureData;
                IsLayerDropDownOpen = true;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling feature service info", ex);
            }
        }

        private void RetrieveServiceInfoCompleted(object sender, RetrieveServiceInfoEventArgs e)
        {
            try
            {
                if(e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling service info", e.Error);
                    return;
                }
                Service = e.ServiceData;
                IsLayerDropDownOpen = true;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling service info", ex);
            }
        }
        #endregion
    }
}
