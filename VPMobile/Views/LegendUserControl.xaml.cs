using Esri.ArcGISRuntime.Mapping;
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
    /// Interaction logic for Legend.xaml
    /// </summary>
    public partial class LegendUserControl : UserControl
    {
        #region public
        #region public constructor
        public LegendUserControl()
        {
            try
            {
                InitializeComponent();
                ViewModel = new LegendViewModel();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error initializing Legend", ex);
            }
        }
        #endregion

        #region public properties
        public LegendViewModel ViewModel
        {
            get { return DataContext as LegendViewModel; }
            set
            {
                DataContext = value;
            }
        }
        #endregion

        #region public events
        #endregion
        #region public methods
        #endregion
        #endregion

        #region private
        #endregion
    }
}
