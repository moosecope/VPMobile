using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
        }
        #endregion

        #region public properties

        public MainViewModel ViewModel
        {
            get { return DataContext as MainViewModel; }
            set
            {
                try
                {
                    var old = DataContext as MainViewModel;
                    if (old != null)
                        old.PropertyChanged -= ViewModel_PropertyChanged;
                    value.RoutingGraphicsLayer = RoutingGraphicsLayer;
                    value.LocationGraphicsLayer = LocationGraphicsLayer;
                    value.Legend = ucLegend.ViewModel;
                    ucLegend.ViewModel.MainView = value;
                    value.AvlList = ucAvl.ViewModel;
                    ucAvl.ViewModel.MainView = value;
                    value.IncidentsList = ucIncidents.ViewModel;
                    ucIncidents.ViewModel.MainView = value;
                    value.UserSettings = ucUserSettings.ViewModel;
                    ucUserSettings.ViewModel.MainView = value;
                    ucFind.ViewModel.MainView = value;
                    value.LocationTools = ucFind.ViewModel;
                    ucSelectConfig.ViewModel.ConfigSelected = value.ConfigSelected;
                    ucSelectConfig.ViewModel.MainView = value;
                    value.MapView = mvwMain;
                    value.PropertyChanged += ViewModel_PropertyChanged;
                    DataContext = value;
                    NotifyPropertyChanged();
                }
                catch (Exception ex)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error setting viewmodel", ex);
                }
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

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "IncidentsVisible" || e.PropertyName == "AvlVisible" || e.PropertyName == "LegendVisible" || e.PropertyName == "RoutingVisible")
                {
                    UserControl first = null, second = null;
                    if (ViewModel.IncidentsVisible)
                    {
                        first = ucIncidents;
                    }
                    if (ViewModel.AvlVisible)
                    {
                        if (first == null)
                            first = ucAvl;
                        else
                            second = ucAvl;
                    }
                    if (ViewModel.LegendVisible)
                    {
                        if (first == null)
                            first = ucLegend;
                        else
                            second = ucLegend;
                    }
                    if (ViewModel.RoutingVisible)
                    {
                        if (first == null)
                            first = ucRouting;
                        else
                            second = ucRouting;
                    }
                    if (first != null && second == null)
                    {
                        first.SetValue(System.Windows.Controls.Grid.RowProperty, 1);
                        first.SetValue(System.Windows.Controls.Grid.RowSpanProperty, 10);
                    }
                    else if (first != null && second != null)
                    {
                        first.SetValue(System.Windows.Controls.Grid.RowProperty, 1);
                        first.SetValue(System.Windows.Controls.Grid.RowSpanProperty, 5);

                        second.SetValue(System.Windows.Controls.Grid.RowProperty, 6);
                        second.SetValue(System.Windows.Controls.Grid.RowSpanProperty, 5);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on property changed", ex);
            }
        }

        private void MapView_LayerViewStateChanged(object sender, LayerViewStateChangedEventArgs e)
        {
            try
            {
                // get the name of the layer that raised the event
                var layerName = e.Layer.Name;
                // check the status and report the value
                switch (e.LayerViewState.Status)
                {
                    case LayerViewStatus.Active:
                        break;
                    case LayerViewStatus.Error:
                        Console.WriteLine("Error displaying " + layerName);
                        break;
                    case LayerViewStatus.Loading:
                        break;
                    case LayerViewStatus.NotVisible:
                        Console.WriteLine(layerName + " is not visible.");
                        break;
                    case LayerViewStatus.OutOfScale:
                        Console.WriteLine(layerName + " is not displayed due to the current scale.");
                        break;
                    default:
                        Console.WriteLine("Status of " + layerName + " is unknown.");
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on layer view state changed", ex);
            }
        }

        private void MapView_DrawStatusChanged(object sender, DrawStatusChangedEventArgs e)
        {
            try
            {
                // if drawing is complete, hide the activity indicator
                if (e.Status == DrawStatus.Completed)
                {
                    Console.WriteLine("Map view drawing is complete.");
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on map view draw status changed", ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.StartSplashScreenUpdate();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on window load", ex);
            }
        }

        private void Incidents_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.IncidentsVisible = !ViewModel.IncidentsVisible;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on incident mouse up", ex);
            }
        }

        private void Avl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.AvlVisible = !ViewModel.AvlVisible;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on avl mouse up", ex);
            }
        }

        private void Legend_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.LegendVisible = !ViewModel.LegendVisible;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on legend mouse up", ex);
            }
        }

        private void Routing_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.RoutingVisible = !ViewModel.RoutingVisible;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on routing mouse up", ex);
            }
        }

        private void Identify_Close_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.IdentifyVisible = false;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on routing mouse up", ex);
            }
        }

        private void Initial_Extent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.ZoomToFullExtent();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on initial extent mouse up", ex);
            }
        }

        private void Zoom_In_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.ZoomIn();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on zoom in mouse up", ex);
            }
        }

        private void Zoom_Out_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.ZoomOut();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on zoom out mouse up", ex);
            }
        }

        private void Location_Tools_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.FindVisible = false;
                ViewModel.SettingsVisible = false;
                ViewModel.FindButtonsVisible = !ViewModel.FindButtonsVisible;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on location tools mouse up", ex);
            }
        }

        private void Find_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.FindVisible = !ViewModel.FindVisible;
                ViewModel.FindButtonsVisible = false;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on find mouse up", ex);
            }
        }

        private void Identify_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.Identify();
                ViewModel.FindButtonsVisible = false;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on identify mouse up", ex);
            }
        }

        private void Setting_Buttons_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.FindVisible = false;
                ViewModel.SettingsVisible = false;
                ViewModel.SettingsButtonsVisible = !ViewModel.SettingsButtonsVisible;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on settings button mouse up", ex);
            }
        }

        private void Avl_Visibility_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.AvlGraphicsLayer.IsVisible = !ViewModel.AvlGraphicsLayer.IsVisible;
                ViewModel.SettingsButtonsVisible = false;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on avl visibility mouse up", ex);
            }
        }

        private void Base_Map_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.ToggleBaseMap();
                ViewModel.SettingsButtonsVisible = false;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on base map mouse up", ex);
            }
        }

        private void Night_Mode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.ToggleNightMode();
                ViewModel.SettingsButtonsVisible = false;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on night mode mouse up", ex);
            }
        }

        private void Help_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.ViewHelpDocument();
                ViewModel.SettingsButtonsVisible = false;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on help mouse up", ex);
            }
        }

        private void Settings_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.SettingsVisible = !ViewModel.SettingsVisible;
                ViewModel.SettingsButtonsVisible = false;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on settings mouse up", ex);
            }
        }

        private void Identify_Feature_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var border = e.OriginalSource as Border;
                if (border != null)
                    ViewModel.Show(((Feature)border.DataContext).Geometry, false);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on identify feature click", ex);
            }
        }

        private void Clear_Routing_Data_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearRoutingInstructions();
        }

        private void GPS_Center_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.ZoomToGps();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on gps center mouse up", ex);
            }
        }

        private void GPS_Keep_Center_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.GpsKeepCentered = !ViewModel.GpsKeepCentered;
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on gps keep centered mouse up", ex);
            }
        }
        #endregion
    }
}
