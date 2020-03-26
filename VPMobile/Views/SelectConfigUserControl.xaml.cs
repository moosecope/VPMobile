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
    /// Interaction logic for SelectConfigUserControl.xaml
    /// </summary>
    public partial class SelectConfigUserControl : UserControl, INotifyPropertyChanged
    {
        public SelectConfigUserControl()
        {
            try
            {
                InitializeComponent();
                ViewModel = new SelectConfigViewModel();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error initializing select config user control", ex);
            }
        }

        public SelectConfigViewModel ViewModel
        {
            get { return DataContext as SelectConfigViewModel; }
            set
            {
                DataContext = value;
                NotifyPropertyChanged();
            }
        }

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region private
        //  This method is called by the Set accessor of each property.
        //  The CallerMemberName attribute that is applied to the optional propertyName
        //  parameter causes the property name of the caller to be substituted as an argument.
        //  Note: Requires Framework 4.5 or higher
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.SelectConfig();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on select click", ex);
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.SelectConfig();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on config double click", ex);
            }
        }
        #endregion
    }
}
