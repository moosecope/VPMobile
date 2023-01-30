using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPMobileAdmin.ViewModels
{

    public class DispatchSettingsRootobject
    {
        public VPMobileAdmin.VPCoreService.VPMobileDispatchSettings[] GetAvailableDispatchSettingsResult { get; set; }
    }

    public class AvlSettingsRootobject
    {
        public VPMobileAdmin.VPCoreService.VPMobileAVLSettings[] GetAvailableAvlSettingsResult { get; set; }
    }

}