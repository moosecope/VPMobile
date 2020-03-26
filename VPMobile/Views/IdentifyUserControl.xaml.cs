using Esri.ArcGISRuntime.Data;
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

namespace VP_Mobile.Views
{
    /// <summary>
    /// Interaction logic for IdentifyUserControl.xaml
    /// </summary>
    public partial class IdentifyUserControl : UserControl, INotifyPropertyChanged
    {
        public IdentifyUserControl()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to map coordinate click", ex);
            }
        }

        public ObservableCollection<Feature> IdentifyFeatures
        {
            get { return (ObservableCollection<Feature>)this.GetValue(IncidentProperty); }
            set { this.SetValue(IncidentProperty, value); }
        }
        public static readonly DependencyProperty IncidentProperty = DependencyProperty.Register(
            "IdentifyFeatures", typeof(ObservableCollection<Feature>), typeof(IdentifyUserControl));

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;

        public event RoutedEventHandler IdentifyFeatureClick
        {
            add { AddHandler(IdentifyFeatureEvent, value); }
            remove { RemoveHandler(IdentifyFeatureEvent, value); }
        }
        public static readonly RoutedEvent IdentifyFeatureEvent = EventManager.RegisterRoutedEvent(
            "IdentifyFeatureClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IdentifyUserControl));
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

        private void Feature_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(IdentifyFeatureEvent, sender));
        }
        #endregion
    }
}
