using Esri.ArcGISRuntime.Mapping;
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
    /// Interaction logic for Legend.xaml
    /// </summary>
    public partial class Legend : UserControl
    {
        public LegendViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }

        public Legend()
        {
            InitializeComponent();
        }
    }
}
