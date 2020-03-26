using GTG.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VP_Mobile.StaticHelpers;

namespace VP_Mobile.ViewModels
{
    public class AvlListViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public AvlListViewModel()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                AvlGroups = new ObservableCollection<AvlGroupViewModel>();
            }
            catch (Exception ex)
            {
                var message = "Error initializing Avl List View Model";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion

        #region public properties
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

        private ObservableCollection<AvlGroupViewModel> _avlGroups;
        public ObservableCollection<AvlGroupViewModel> AvlGroups
        {
            get { return _avlGroups; }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_avlGroups != null)
                        _avlGroups.CollectionChanged -= AvlGroups_CollectionChanged;
                    _avlGroups = value;
                    if (_avlGroups != null)
                        _avlGroups.CollectionChanged += AvlGroups_CollectionChanged;
                    NotifyPropertyChanged();
                }
                catch (Exception ex)
                {
                    var message = "Error setting AvlGroups";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region public methods

        public void AvlUnitSelected(AvlViewModel avlUnit)
        {

            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                    { nameof(avlUnit), avlUnit }
                });
                MainView.ZoomTo(new Models.Point
                {
                    Latitude = avlUnit.Latitude,
                    Longitude = avlUnit.Longitude
                });
            }
            catch (Exception ex)
            {
                var message = "Error zooming to avl unit";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }

        }
        #endregion
        #endregion

        #region private

        //  This method is called by the Set accessor of each property.
        //  The CallerMemberName attribute that is applied to the optional propertyName
        //  parameter causes the property name of the caller to be substituted as an argument.
        //  Note: Requires Framework 4.5 or higher
        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AvlGroups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                    { nameof(e.Action), e.Action }
                });
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems.Cast<AvlGroupViewModel>())
                        {
                            item.PropertyChanged += AvlGroup_PropertyChanged;
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.NewItems.Cast<AvlGroupViewModel>())
                        {
                            item.PropertyChanged -= AvlGroup_PropertyChanged;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = "Error on AvlGroups changed";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void AvlGroup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                    { nameof(e.PropertyName), e.PropertyName }
                });
                if (e.PropertyName == nameof(AvlGroupViewModel.Visible))
                    MainView.UpdateAvlUnits();
            }
            catch (Exception ex)
            {
                var message = "Error on AvlGroup changed";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void MainView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(MainView));
        }
        #endregion
    }
}
