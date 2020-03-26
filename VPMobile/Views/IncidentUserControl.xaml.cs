using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for IncidentUserControl.xaml
    /// </summary>
    public partial class IncidentUserControl : UserControl
    {
        #region public
        #region public constructor
        public IncidentUserControl()//TODO - fix change in image margins on load
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
        #endregion

        #region public properties
        #endregion

        #region public events
        #endregion
        #region public methods
        #endregion
        public IncidentViewModel Incident
        {
            get { return (IncidentViewModel)this.GetValue(IncidentProperty); }
            set { this.SetValue(IncidentProperty, value); }
        }
        public static readonly DependencyProperty IncidentProperty = DependencyProperty.Register(
            "Incident", typeof(IncidentViewModel), typeof(IncidentUserControl));
        
        public event RoutedEventHandler Selected
        {
            add { AddHandler(SelectedEvent, value); }
            remove { RemoveHandler(SelectedEvent, value); }
        }
        public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent(
            "Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IncidentUserControl));

        public event RoutedEventHandler RouteTo
        {
            add { AddHandler(RouteToEvent, value); }
            remove { RemoveHandler(RouteToEvent, value); }
        }
        public static readonly RoutedEvent RouteToEvent = EventManager.RegisterRoutedEvent(
            "RouteTo", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IncidentUserControl));
        #endregion

        #region private

        private void thisControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Incident.Selected)
                    imgCallType.Margin = new Thickness(6, 6, 6, 0);
                else
                    imgCallType.Margin = new Thickness(6);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on incident user control loaded", ex);
            }
        }

        private void Incident_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RaiseEvent(new RoutedEventArgs(SelectedEvent, this));
                if (Incident.Selected)
                    imgCallType.Margin = new Thickness(6, 6, 6, 0);
                else
                    imgCallType.Margin = new Thickness(6);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on incident mouse up", ex);
            }
        }

        private void Route_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RaiseEvent(new RoutedEventArgs(RouteToEvent, this));
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to mouse up", ex);
            }
        }
        #endregion
    }
}
