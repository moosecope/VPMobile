using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPMobileRuntime100_1_0.Model
{
    public interface GraphicsLayerDataSource
    {
        /// <summary>
        /// The graphics to show on the map
        /// </summary>
        ObservableCollection<Graphic> Graphics { get; }

        /// <summary>
        /// A dictionary containing the render value and path to the image
        /// </summary>
        IDictionary<String, PictureSymbol> LayerRenderer { get; }

        /// <summary>
        /// The default image for render values not found in LayerRenderer
        /// </summary>
        String DefaultImage { get; }

        /// <summary>
        /// The wkid of the graphics in this layer
        /// </summary>
        int WKID { get; }
    }
}
