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
    /// Interaction logic for RoutingUserControl.xaml
    /// </summary>
    public partial class RoutingUserControl : UserControl, INotifyPropertyChanged
    {
        public RoutingUserControl()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error initializing routing user control", ex);
            }
        }

        public ObservableCollection<RoutingViewModel> RoutingInstructions
        {
            get { return (ObservableCollection<RoutingViewModel>)this.GetValue(IncidentProperty); }
            set { this.SetValue(IncidentProperty, value); }
        }
        public static readonly DependencyProperty IncidentProperty = DependencyProperty.Register(
            "RoutingInstructions", typeof(ObservableCollection<RoutingViewModel>), typeof(RoutingUserControl));

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;

        public event RoutedEventHandler ClearRoutingDataClick
        {
            add { AddHandler(ClearEvent, value); }
            remove { RemoveHandler(ClearEvent, value); }
        }
        public static readonly RoutedEvent ClearEvent = EventManager.RegisterRoutedEvent(
            "ClearRoutingDataClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RoutingUserControl));
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

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RaiseEvent(new RoutedEventArgs(ClearEvent, this));
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on clear click", ex);
            }
        }
        #endregion
    }
}
