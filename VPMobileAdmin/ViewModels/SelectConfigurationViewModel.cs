using log4net;
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
using VPMobileAdmin.StaticHelpers;
using VPMobileAdmin.Views;
using VPMobileAdmin.VPMobileService;
using VPMobileObjects;

namespace VPMobileAdmin.ViewModels
{
    public class SelectConfigurationViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public SelectConfigurationViewModel()
        {
            _mobileService = new VPMobileServiceClient();
            _mobileService.GetConfigsCompleted += GetConfigsCompleted;
            _mobileService.GetConfigCompleted += GetConfigCompleted;
            _mobileService.DeleteConfigCompleted += DeleteConfigCompleted;
            _mobileService.GetConfigsAsync(Guid.NewGuid().ToString());
        }
        #endregion

        #region public properties

        private ObservableCollection<Tuple<String, String>> _configurations;
        public ObservableCollection<Tuple<String,String>> Configurations
        {
            get { return _configurations; }
            set
            {
                _configurations = value;
                NotifyPropertyChanged();
            }
        }

        private SelectConfigurationWindow _view;
        public SelectConfigurationWindow View
        {
            get { return _view; }
            set
            {
                _view = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region public methods
        public void Add_Configuration()
        {
            try
            {
                var dlgConfiguration = new EditConfigurationWindow();
                dlgConfiguration.ViewModel.Configuration = new VPMobileSettings();
                dlgConfiguration.Owner = View;
                dlgConfiguration.ShowDialog();
                _mobileService.GetConfigsAsync(Guid.NewGuid().ToString());
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error adding cache settings", ex);
            }
        }

        public void Edit_Configuration(int index)
        {
            try
            {
                if (index < 0 || index >= Configurations.Count)
                    return;

                _mobileService.GetConfigAsync(Guid.NewGuid().ToString(), Configurations[index].Item1);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling service info", ex);
            }
        }

        public void Delete_Configuration(int index)
        {
            try
            {
                if (index < 0 || index >= Configurations.Count || ErrorHelper.OnMessage("Are you sure you want to delete this configuration?", "Delete", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;

                _mobileService.DeleteConfig(Guid.NewGuid().ToString(), Configurations[index].Item1);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error pulling service info", ex);
            }
        }
        #endregion
        #endregion

        #region private
        private VPMobileServiceClient _mobileService;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //  This method is called by the Set accessor of each property.
        //  The CallerMemberName attribute that is applied to the optional propertyName
        //  parameter causes the property name of the caller to be substituted as an argument.
        //  Note: Requires Framework 4.5 or higher
        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GetConfigsCompleted(object sender, GetConfigsCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error getting configuration", e.Error);
                    return;
                }
                if(e.Result != null)
                    Configurations = new ObservableCollection<Tuple<String, String>>(e.Result);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error getting configuration", ex);
            }
        }

        private void GetConfigCompleted(object sender, GetConfigCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error getting configuration", e.Error);
                    return;
                }
                var dlgConfiguration = new EditConfigurationWindow();
                dlgConfiguration.Owner = View;
                dlgConfiguration.ViewModel.Configuration = e.Result;

               
                _mobileService.GetConfigsAsync(Guid.NewGuid().ToString());                

                dlgConfiguration.ShowDialog();
              
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error getting configuration", ex);
            }
        }

        private void DeleteConfigCompleted(object sender, DeleteConfigCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error deleting configuration", e.Error);
                    return;
                }

                _mobileService.GetConfigsAsync(Guid.NewGuid().ToString());
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error deleting configuration", ex);
            }
        }
        #endregion
    }
}
