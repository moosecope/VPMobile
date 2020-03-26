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
using VP_Mobile.StaticHelpers;
using GTG.Utilities;
using System.Reflection;

namespace VP_Mobile.ViewModels
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
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(name), name }
                    });
                this.Name = name;
                this.Children = new ObservableCollection<TreeViewItem>();
                Children.CollectionChanged += Children_CollectionChanged;
            }
            catch (Exception ex)
            {
                var message = "Error initializing TreeViewItem";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(e.Action), e.Action }
                    });
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (TreeViewItem newItem in e.NewItems)
                    {
                        newItem._parent = this;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "Error on children collection changed";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public TreeViewItem(string name, ImageSource imageSource)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(name), name }
                    });
                this.Name = name;
                MirrorToParent = false;
                this.Children = new ObservableCollection<TreeViewItem>();
                Children.CollectionChanged += Children_CollectionChanged;
                this.Image = imageSource;
            }
            catch (Exception ex)
            {
                var message = "Error initializing TreeViewItem";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public TreeViewItem(string name, Symbol symbol)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(name), name }
                    });
                this.Name = name;
                MirrorToParent = false;
                this.Children = new ObservableCollection<TreeViewItem>();
                Children.CollectionChanged += Children_CollectionChanged;
                SetSymbol(symbol);
            }
            catch (Exception ex)
            {
                var message = "Error initializing TreeViewItem";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
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
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(value), value },
                        { nameof(updateChildren), updateChildren },
                        { nameof(updateParent), updateParent }
                    });
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
            catch (Exception ex)
            {
                var message = "Error setting IsChecked";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        void VerifyCheckState()
        {

            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
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
            catch (Exception ex)
            {
                var message = "Error verifying check state";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
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
