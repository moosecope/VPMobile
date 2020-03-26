using Esri.ArcGISRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPMobileRuntime100_1_0
{
    public static class VPMobileRuntime
    {
        public const String ARCGIS_LITE_LICENSE_KEY = "runtimelite,1000,rud3102912412,none,6PB3LNBHPDMF8YAJM245";

        public static void Initialize()
        {
            ArcGISRuntimeEnvironment.Initialize();
            ArcGISRuntimeEnvironment.SetLicense(ARCGIS_LITE_LICENSE_KEY);
        }
    }
}
