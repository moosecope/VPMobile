using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
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
using VPMobileRuntime100_1_0.ViewModel;

namespace VPMobileRuntime100_1_0.View
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        public MapViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }

        public MapView()
        {
            InitializeComponent();
        }

        private void MapView_LayerViewStateChanged(object sender, LayerViewStateChangedEventArgs e)
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

        private void MapView_DrawStatusChanged(object sender, DrawStatusChangedEventArgs e)
        {
            // if drawing is complete, hide the activity indicator
            if (e.Status == DrawStatus.Completed)
            {
                Console.WriteLine("Map view drawing is complete.");
            }
        }
    }
}
