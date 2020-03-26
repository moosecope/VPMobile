using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VP_Mobile.ViewModels
{
    public class RoutingViewModel
    {
        #region public
        #region public constructor
        public RoutingViewModel(String routeText)
        {
            RouteText = routeText;
        }
        #endregion

        #region public properties
        public String RouteText { get; set; }
        #endregion        
        #endregion
    }
}
