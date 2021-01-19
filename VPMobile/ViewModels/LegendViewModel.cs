using Esri.ArcGISRuntime.Mapping;
using GTG.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using VP_Mobile.StaticHelpers;

namespace VP_Mobile.ViewModels
{
    public class LegendViewModel : INotifyPropertyChanged
    {
        public LegendViewModel()
        {
            Layers = new List<Layer>();
        }

        private MainViewModel _mainView;
        public MainViewModel MainView
        {
            get { return _mainView; }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_mainView != null)
                        _mainView.PropertyChanged -= MainView_PropertyChanged;
                    _mainView = value;
                    _mainView.PropertyChanged += MainView_PropertyChanged;
                    NotifyPropertyChanged();
                }
                catch (Exception ex)
                {
                    var message = "Error setting MainView";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private IEnumerable<Layer> _layers;
        public IEnumerable<Layer> Layers
        {
            get { return _layers; }
            set
            {
                if (value != null)
                    _layers = value.Reverse();
                else
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
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_root == null)
                    {
                        _root = new ObservableCollection<TreeViewItem>();
                        int layerOrder = 0;
                        foreach (Layer mapLayer in Layers)
                        {
                            Console.WriteLine("Processing " + mapLayer.Name + " Load Status = " + mapLayer.LoadStatus);
                            if (mapLayer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                                AddLayerTree(mapLayer, layerOrder);
                            else
                            {
                                mapLayer.LoadStatusChanged += (sender, e) =>
                                {
                                    if (e.Status != Esri.ArcGISRuntime.LoadStatus.Loaded)
                                        return;
                                    Application.Current.Dispatcher.Invoke((() => AddLayerTree(mapLayer, layerOrder)));
                                };
                            }
                            layerOrder++;
                        }
                    }
                    return _root;
                }
                catch (Exception ex)
                {
                    var message = "Error pulling Root";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                    return _root;
                }
            }
            private set
            {
                _root = value;
                NotifyPropertyChanged();
            }
        }

        private async void AddLayerTree(Layer layer, int atIndex)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(layer), layer }
                    });
                Console.WriteLine("Adding " + layer.Name);
                var service = new TreeViewItem(layer.Name)
                {
                    CanDisable = true,
                    IsChecked = layer.IsVisible
                };
                service = await TraverseLayer(layer, service);
                if (_root == null)
                    return;
                // TraverseLayer is async so the items likely will not load in proper order
                // ue the passed in order to add them at the correct position
                if (atIndex < _root.Count)
                {
                    _root.Insert(atIndex, service);
                }
                else
                {
                    _root.Add(service);
                }
                Console.WriteLine("Added " + layer.Name);
            }
            catch (Exception ex)
            {
                var message = "Error building legend";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private async Task<TreeViewItem> TraverseLayer(ILayerContent layer, TreeViewItem root)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(layer), layer },
                        { nameof(root), root }
                    });
                root.CanDisable = layer.CanChangeVisibility;
                root.IsChecked = layer.IsVisible;
                root.MirrorToParent = false;
                root.Layer = layer;
                IReadOnlyList<LegendInfo> lgnd = await layer.GetLegendInfosAsync();
                if (lgnd.Count() == 1)
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
                        child = await TraverseLayer(subLyr, new TreeViewItem(subLyr.Name));
                    else
                        child = await TraverseLayer(subLyr, new TreeViewItem(subLyr.Name, info.Symbol));
                    root.Children.Add(child);
                }
                return root;
            }
            catch (Exception ex)
            {
                var message = "Error traversing layer";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
                return root;
            }
        }

        /// <summary>
        /// Raises the <see cref="MainViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("MainView");
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
