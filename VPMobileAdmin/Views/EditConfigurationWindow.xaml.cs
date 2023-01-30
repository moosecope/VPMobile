using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VPMobileAdmin.StaticHelpers;
using VPMobileAdmin.ViewModels;

namespace VPMobileAdmin.Views
{
    /// <summary>
    /// Interaction logic for EditConfiguration.xaml
    /// </summary>
    public partial class EditConfigurationWindow : Window, INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public EditConfigurationWindow()
        {
            try
            {
                InitializeComponent();
                ViewModel = new EditConfigurationViewModel();
                ViewModel.View = this;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(this.GetType().Name, "Error while initializing", ex);
            }
        }
        #endregion

        #region public properties

        public EditConfigurationViewModel ViewModel
        {
            get { return DataContext as EditConfigurationViewModel; }
            set
            {
                if (DataContext != null)
                    ((EditConfigurationViewModel)DataContext).PropertyChanged -= PropertyChanged;
                DataContext = value;
                if (DataContext != null)
                    value.PropertyChanged += PropertyChanged;
            }
        }
        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        #endregion

        #region private
        //used to check if there have been any changes to the windows properties that need to be saved
        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ThisWindow_Closing(object sender, CancelEventArgs e)
        {
            if (ViewModel != null)
            {
                e.Cancel = ViewModel.OnClosing();
            }
            else
            {
                e.Cancel = false;
            }
        }

        #region Services
        private void lstServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnServiceEdit.IsEnabled = lstServices.SelectedIndex >= 0;
            btnServiceRemove.IsEnabled = lstServices.SelectedIndex >= 0;
            btnServiceUp.IsEnabled = lstServices.SelectedIndex >= 1;
            btnServiceDown.IsEnabled = lstServices.SelectedIndex < lstServices.Items.Count - 1;
        }

        private void Add_Service_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddCacheSettings();
        }

        private void btnServiceEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstServices.SelectedIndex < 0)
                {
                    MessageBox.Show("A service must be selected to edit.");
                    return;
                }
                ViewModel.EditCacheSettings(lstServices.SelectedIndex);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error editing cache settings", ex);
            }
        }

        private void btnServiceRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstServices.SelectedIndex < 0)
                {
                    MessageBox.Show("A service must be selected to remove.");
                    return;
                }
                ViewModel.RemoveCacheSettings(lstServices.SelectedIndex);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error removing cache settings", ex);
            }
        }

        private void btnServiceUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstServices.SelectedIndex < 0)
                {
                    MessageBox.Show("A service must be selected to move.");
                    return;
                }
                ViewModel.MoveCacheSettingsUp(lstServices.SelectedIndex);
                lstServices_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error moving cache settings up", ex);
            }
        }

        private void btnServiceDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstServices.SelectedIndex < 0)
                {
                    MessageBox.Show("A service must be selected to move.");
                    return;
                }
                ViewModel.MoveCacheSettingsDown(lstServices.SelectedIndex);
                lstServices_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error moving cache settings down", ex);
            }
        }
        #endregion

        #region Location Tools

        private void lstGeocoders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnGeocoderEdit.IsEnabled = lstGeocoders.SelectedIndex >= 0;
            btnGeocoderRemove.IsEnabled = lstGeocoders.SelectedIndex >= 0;
        }

        private void Add_Geocoder_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddGeocoder();
        }

        private void Edit_Geocoder_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditGeocoder(lstGeocoders.SelectedIndex);
        }

        private void Remove_Geocoder_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveGeocoder(lstGeocoders.SelectedIndex);
        }

        private void lstStIntersections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnFinderRemove.IsEnabled = lstStIntersections.SelectedIndex >= 0;
        }

        private void Add_Finder_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddStreetIntersectionFinder();
        }

        private void Remove_Finder_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveStreetIntersectionFinder(lstStIntersections.SelectedIndex);
        }
        #endregion

        #endregion

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveConfiguration();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
