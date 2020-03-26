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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VP_Mobile.StaticHelpers;
using VP_Mobile.ViewModels;

namespace VP_Mobile.Views
{
    /// <summary>
    /// Interaction logic for IncidentsListUserControl.xaml
    /// </summary>
    public partial class IncidentsListUserControl : UserControl, INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public IncidentsListUserControl()
        {
            try
            {
                InitializeComponent();
                ViewModel = new IncidentsListViewModel();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to map coordinate click", ex);
            }
        }
        #endregion

        #region public properties

        public IncidentsListViewModel ViewModel
        {
            get { return DataContext as IncidentsListViewModel; }
            set
            {
                DataContext = value;
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

        //  This method is called by the Set accessor of each property.
        //  The CallerMemberName attribute that is applied to the optional propertyName
        //  parameter causes the property name of the caller to be substituted as an argument.
        //  Note: Requires Framework 4.5 or higher
        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.CallTypeToggle((CallTypeViewModel)((Image)sender).DataContext);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to map coordinate click", ex);
            }
        }

        private void Incident_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                var userControl = sender as IncidentUserControl;
                if (userControl != null)
                    ViewModel.IncidentToggle(userControl.Incident);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to map coordinate click", ex);
            }
        }

        private void Incident_RouteTo(object sender, RoutedEventArgs e)
        {
            try
            {
                var userControl = sender as IncidentUserControl;
                if (userControl != null)
                    ViewModel.IncidentRouteTo(userControl.Incident);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to map coordinate click", ex);
            }
        }
        #endregion
    }
}
