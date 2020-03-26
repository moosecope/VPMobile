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
    /// Interaction logic for LocationToolUserControl.xaml
    /// </summary>
    public partial class LocationToolUserControl : UserControl, INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public LocationToolUserControl()
        {
            try
            {
                InitializeComponent();
                ViewModel = new LocationToolViewModel();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error initializing location tools user control", ex);
            }
        }
        #endregion

        #region public properties
        public LocationToolViewModel ViewModel
        {
            get { return DataContext as LocationToolViewModel; }
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

        #region Map Coordinates
        private void Show_Map_Coordinate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ShowCoordinateOnMap();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on show map coordinate click", ex);
            }
        }

        private void Find_Map_Coordinate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.FindCoordinateFromMap();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on find map coordinate click", ex);
            }
        }

        private void Route_To_Map_Coordinate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.RouteToCoordinate();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to map coordinate click", ex);
            }
        }
        #endregion

        #region Street Intersections
        private void Find_Street_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ShowStreetOnMap();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on find street click", ex);
            }
        }

        private void Find_Intersection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ShowIntersectionOnMap();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on find intersection click", ex);
            }
        }

        private void Route_To_Intersection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.RouteToIntersection();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to intersection click", ex);
            }
        }
        #endregion

        #region Points of Interest
        private void Add_Poi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.AddPoi();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on add poi click", ex);
            }
        }

        private void Remove_Poi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.RemovePoi();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on remove poi click", ex);
            }
        }

        private void Find_Poi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ShowPoi();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on find poi click", ex);
            }
        }

        private void Route_To_Poi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.RouteToPoi();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to poi click", ex);
            }
        }
        #endregion

        #region Addresses
        private void Generate_Address_Candidates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.GenerateCandidates();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on generate address candidates click", ex);
            }
        }

        private void Find_Address_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ShowAddress();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on find address click", ex);
            }
        }

        private void Route_To_Address_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.RouteToAddress();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on route to address click", ex);
            }
        }
        #endregion

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ClearFindResults();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error on clear find marks click", ex);
            }
        }
        #endregion
    }
}
