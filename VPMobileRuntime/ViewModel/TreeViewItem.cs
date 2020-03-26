using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Esri.ArcGISRuntime.Symbology;
using System.Collections.ObjectModel;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System.Runtime.CompilerServices;

namespace VPMobileRuntime100_1_0.ViewModel
{
    public class TreeViewItem : INotifyPropertyChanged
    {
        #region Data

        bool? _isChecked = false;
        bool _canDisable = false;
        TreeViewItem _parent;

        #endregion

        public TreeViewItem(string name)
        {
            this.Name = name;
            this.Children = new ObservableCollection<TreeViewItem>();
            Children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (TreeViewItem newItem in e.NewItems)
                {
                    newItem._parent = this;
                }
            }
        }

        public TreeViewItem(string name, ImageSource imageSource)
        {
            this.Name = name;
            MirrorToParent = false;
            this.Children = new ObservableCollection<TreeViewItem>();
            Children.CollectionChanged += Children_CollectionChanged;
            this.Image = imageSource;
        }

        public TreeViewItem(string name, Symbol symbol)
        {
            this.Name = name;
            MirrorToParent = false;
            this.Children = new ObservableCollection<TreeViewItem>();
            Children.CollectionChanged += Children_CollectionChanged;
            SetSymbol(symbol);
        }

        #region Properties

        public ObservableCollection<TreeViewItem> Children { get; private set; }

        public bool IsInitiallySelected { get; private set; }

        public string Name { get; set; }

        public ImageSource Image { get; private set; }

        public async void SetSymbol(Symbol symbol)
        {
            var swatch = await symbol.CreateSwatchAsync();

            Image = await RuntimeImageExtensions.ToImageSourceAsync(swatch);
        }

        public ILayerContent Layer { get; set; }

        public bool CanDisable
        {
            get { return _canDisable; }
            set
            {
                _canDisable = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool MirrorToParent;

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child LegendViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (MirrorToParent)
                {
                    _parent.IsChecked = !_parent.IsChecked;
                    return;
                }
                this.SetIsChecked(value, true, true);
            }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (CanDisable && Layer != null)
                Layer.IsVisible = value.GetValueOrDefault(true);

            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
            {
                foreach (var child in Children)
                {
                    child.SetIsChecked(IsChecked, true, false);
                }
            }

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();
            this.NotifyPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises the <see cref="MainViewModel.PropertyChanged" /> event
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
