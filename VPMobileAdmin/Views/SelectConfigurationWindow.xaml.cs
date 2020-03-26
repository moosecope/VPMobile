using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using VPMobileAdmin.ViewModels;

namespace VPMobileAdmin.Views
{
    /// <summary>
    /// Interaction logic for SelectConfigurationWindow.xaml
    /// </summary>
    public partial class SelectConfigurationWindow : Window, INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public SelectConfigurationWindow()
        {
            InitializeComponent();
            ViewModel = new SelectConfigurationViewModel();
            ViewModel.View = this;
        }
        #endregion

        #region public properties

        public SelectConfigurationViewModel ViewModel
        {
            get
            {
                return DataContext as SelectConfigurationViewModel;
            }
            set
            {
                DataContext = value;
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

        private void lstConfigurations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnConfigurationEdit.IsEnabled = lstConfigurations.SelectedIndex >= 0;
            btnConfigurationDelete.IsEnabled = lstConfigurations.SelectedIndex >= 0;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Add_Configuration();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Edit_Configuration(lstConfigurations.SelectedIndex);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Delete_Configuration(lstConfigurations.SelectedIndex);
        }
        #endregion
    }
}
