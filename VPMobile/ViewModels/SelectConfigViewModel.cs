using GTG.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VP_Mobile.Models;
using VP_Mobile.StaticHelpers;
using VP_Mobile.VPMobileService;
using VPMobileObjects;

namespace VP_Mobile.ViewModels
{
    public class SelectConfigViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public SelectConfigViewModel()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.LastConfig))
                    SelectedConfig = Properties.Settings.Default.LastConfig;
            }
            catch (Exception ex)
            {
                var message = "Error initializing view model";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion

        #region public properties

        private ObservableCollection<Tuple<String, String>> _configurations;
        public ObservableCollection<Tuple<String, String>> Configurations
        {
            get { return _configurations; }
            set
            {
                _configurations = value;
                NotifyPropertyChanged();
            }
        }

        private String _selectedConfig;
        public String SelectedConfig
        {
            get { return _selectedConfig; }
            set
            {
                _selectedConfig = value;
                NotifyPropertyChanged();
            }
        }

        private MainViewModel _mainView;
        public MainViewModel MainView
        {
            get { return _mainView; }
            set
            {

                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_mainView != null)
                        _mainView.PropertyChanged -= MainView_PropertyChanged;
                    _mainView = value;
                    _mainView.PropertyChanged += MainView_PropertyChanged;
                    Disclaimer = value.GpsConfig.IsSungard ? DISCLAMER_SUNGARD : DISCLAMER_NORMAL;
                    _mobileService = new VPMobileServiceClient();
                    _mobileService.GetConfigCompleted += GetConfigCompleted;
                    _mobileService.GetConfigsCompleted += GetConfigsCompleted;
                    _mobileService.GetConfigsAsync(Guid.NewGuid().ToString());
                    NotifyPropertyChanged();
                }
                catch (Exception ex)
                {
                    var message = "Error setting MainView";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private String _disclaimer;
        public String Disclaimer
        {
            get
            {
                if (_disclaimer == null)
                    return DISCLAMER_NORMAL;
                return _disclaimer;
            }
            set
            {
                _disclaimer = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        public Action<VPMobileSettings> ConfigSelected;
        #endregion
        #region public methods
        public void SelectConfig()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (String.IsNullOrWhiteSpace(SelectedConfig))
                    return;

                _mobileService.GetConfigAsync(Guid.NewGuid().ToString(), SelectedConfig);
            }
            catch (Exception ex)
            {
                var message = "Error selecting config";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion
        #endregion

        #region private
        private VPMobileServiceClient _mobileService;
        private const String DISCLAMER_SUNGARD = "DISCLAIMER:  The recommended routes may not be fastest, shortest, or best route. SunGard PS and the third party licensor make no representations or warranties, expressed or implied, with respect to the accuracy, reliability or completeness of the data and are not inviting reliance on these data.  The entire risk as to the use of the software is assumed by the Licensee and Licensee should always verify actual map data and information.  IN NO EVENT WILL SUNGARD PS OR THE THIRD PARTY LICENSOR BE LIABLE TO ANY PARTY FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL OR CONSEQUENTIAL DAMAGES WHETHER RESULTING FROM THE USE, MISUSE, OR INABILITY TO USE THIS PRODUCT, OR FROM DEFECTS IN THE PRODUCT.";
        private const String DISCLAMER_NORMAL = "DISCLAIMER:  The recommended routes may not be fastest, shortest, or best route. Geographic Technologies Group (GTG) makes no representations or warranties, expressed or implied, with respect to the accuracy, reliability or completeness of the data and are not inviting reliance on these data.  The entire risk as to the use of the software is assumed by the Licensee and Licensee should always verify actual map data and information.  IN NO EVENT WILL GTG BE LIABLE TO ANY PARTY FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL OR CONSEQUENTIAL DAMAGES WHETHER RESULTING FROM THE USE, MISUSE, OR INABILITY TO USE THIS PRODUCT, OR FROM DEFECTS IN THE PRODUCT.";


        //  This method is called by the Set accessor of each property.
        //  The CallerMemberName attribute that is applied to the optional propertyName
        //  parameter causes the property name of the caller to be substituted as an argument.
        //  Note: Requires Framework 4.5 or higher
        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("MainView");
        }

        private void GetConfigCompleted(object sender, GetConfigCompletedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (e.Error != null)
                {
                    var message = "Error pulling configuration";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, e.Error);
                    Logging.LogMessage(Logging.LogType.Error, message, e.Error);
                    return;
                }

                ConfigSelected?.Invoke(e.Result);
                Properties.Settings.Default.LastConfig = e.Result.Name;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                var message = "Error pulling configuration";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void GetConfigsCompleted(object sender, GetConfigsCompletedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (e.Error != null)
                {
                    var message = "Error pulling configuration";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, e.Error);
                    Logging.LogMessage(Logging.LogType.Error, message, e.Error);
                    return;
                }

                Configurations = new ObservableCollection<Tuple<string, string>>(e.Result);
                if (Properties.Settings.Default.AutoOpenLastConfig)
                    SelectConfig();
            }
            catch (Exception ex)
            {
                var message = "Error pulling configuration";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion
    }
}
