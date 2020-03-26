using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace VPMobileRuntime100_1_0.ViewModel
{
    public class LegendViewModel : INotifyPropertyChanged
    {
        public LegendViewModel()
        {
            Layers = new List<Layer>();
        }

        private IEnumerable<Layer> _layers;
        public IEnumerable<Layer> Layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                NotifyPropertyChanged();
                Root = null;
            }
        }

        private ObservableCollection<TreeViewItem> _root;
        public ObservableCollection<TreeViewItem> Root
        {
            get
            {
                if(_root == null)
                {
                    _root = new ObservableCollection<TreeViewItem>();
                    foreach (Layer mapLayer in Layers)
                    {
                        var service = new TreeViewItem(mapLayer.Name)
                        {
                            CanDisable = true,
                            IsChecked = mapLayer.IsVisible
                        };
                        service = TraverseLayer(mapLayer, service);
                        _root.Add(service);
                    }
                }
                return _root;
            }
            private set
            {
                _root = value;
                NotifyPropertyChanged();
            }
        }

        private TreeViewItem TraverseLayer(ILayerContent layer, TreeViewItem root)
        {
            root.CanDisable = layer.CanChangeVisibility;
            root.IsChecked = layer.IsVisible;
            root.MirrorToParent = false;
            Console.WriteLine(layer.SublayerContents.Count);
            root.Layer = layer;
            var lgnd = layer.GetLegendInfosAsync().Result;
            if (lgnd.Count == 1)
                root.SetSymbol(lgnd[0].Symbol);
            else
            {
                foreach (var smbl in lgnd)
                {
                    var item = new TreeViewItem(smbl.Name, smbl.Symbol);
                    item.CanDisable = false;
                    root.Children.Add(item);
                }
            }
            foreach (var subLyr in layer.SublayerContents)
            {
                var info = lgnd.FirstOrDefault(inf => inf.Name == subLyr.Name);
                TreeViewItem child = null;
                if (info == null)
                    child = TraverseLayer(subLyr, new TreeViewItem(subLyr.Name));
                else
                    child = TraverseLayer(subLyr, new TreeViewItem(subLyr.Name, info.Symbol));
                root.Children.Add(child);
            }
            return root;
        }

        /// <summary>
        /// Raises the <see cref="MainViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
