using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPMobileRuntime100_1_0.Model;

namespace VPMobileRuntime100_1_0.ViewModel
{
    public class MapViewModel
    {
        public String CachedServicesDirectory { get; set; }

        public ObservableCollection<GraphicsLayerDataSource> GraphicLayers { get; private set; }

        public void AddFeatureService(Geodatabase cache)
        {
            foreach(var table in cache.GeodatabaseFeatureTables)
            {
                var layer = new FeatureLayer(table);
            }
        }

        public void AddTiledCache(TileCache cache)
        {
            var layer = new ArcGISTiledLayer(cache);
        }
    }
}
