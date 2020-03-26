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
    /// Interaction logic for SelectCacheLayerDialog.xaml
    /// </summary>
    public partial class SelectCacheLayerDialog : Window, INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public SelectCacheLayerDialog()
        {
            InitializeComponent();
            ViewModel = new SelectCacheLayerViewModel();
        }
        #endregion

        #region public properties

        public SelectCacheLayerViewModel ViewModel
        {
            get { return DataContext as SelectCacheLayerViewModel; }
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

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        #endregion
    }
}
