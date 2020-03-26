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
using System.Windows.Data;
using VP_Mobile.Models;
using VP_Mobile.StaticHelpers;

namespace VP_Mobile.ViewModels
{
    public class IncidentsListViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public IncidentsListViewModel()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                _callTypes = new ObservableCollection<CallTypeViewModel>();
                Incidents = new ObservableCollection<IncidentViewModel>();
            }
            catch (Exception ex)
            {
                var message = "Error initializing IncidentsListViewModel";
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

        private ObservableCollection<CallTypeViewModel> _callTypes;
        public ObservableCollection<CallTypeViewModel> CallTypes
        {
            get { return _callTypes; }
            set
            {
                _callTypes = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<IncidentViewModel> _incidents;
        public ObservableCollection<IncidentViewModel> Incidents
        {
            get { return _incidents; }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    _incidents = value;
                    NotifyPropertyChanged();
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        _incidentsView = CollectionViewSource.GetDefaultView(_incidents);
                        _incidentsView.Filter = IncidentFilter;
                        _incidentsView.SortDescriptions.Add(new SortDescription("RecordTime", ListSortDirection.Descending));
                        NotifyPropertyChanged("IncidentsView");
                    });
                }
                catch (Exception ex)
                {
                    var message = "Error setting Incidents";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private ICollectionView _incidentsView;
        public ICollectionView IncidentsView
        {
            get { return _incidentsView; }
        }

        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;

        public void CallTypeToggle(CallTypeViewModel callType)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(callType), callType }
                    });
                callType.CallTypeActive = !callType.CallTypeActive;
                _incidentsView.Refresh();
                MainView.UpdateIncidents();
            }
            catch (Exception ex)
            {
                var message = "Error toggling calltype";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void IncidentToggle(IncidentViewModel incident)
        {

            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(incident), incident }
                    });
                var selected = Incidents.Where(incdnt => incdnt.Selected);
                foreach (var incdnt in selected)
                {
                    incdnt.Selected = false;
                    if (incident == null || incdnt.UniqueID == incident.UniqueID)
                        incident = null;
                }

                if (incident != null)
                {
                    incident.Selected = true;
                    if (incident.UnGeocoded)
                        return;
                    MainView.ZoomTo(new Point
                    {
                        Latitude = incident.Latitude,
                        Longitude = incident.Longitude
                    });
                }
            }
            catch (Exception ex)
            {
                var message = "Error toggling incident";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void IncidentRouteTo(IncidentViewModel incident)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(incident), incident }
                    });
                if (incident.UnGeocoded)
                    return;
                MainView.RouteTo(new Point
                {
                    Latitude = incident.Latitude,
                    Longitude = incident.Longitude
                });
            }
            catch (Exception ex)
            {
                var message = "Error routing to incident";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion
        #region public methods
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

        private void MainView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("MainView");
        }

        private bool IncidentFilter(object item)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(item), item }
                    });
                IncidentViewModel incident = item as IncidentViewModel;
                if (incident == null)
                    return false;
                return !CallTypeViewModel.HiddenCallTypes.Contains(incident.CallType);
            }
            catch (Exception ex)
            {
                var message = "Error filtering incident";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
                return false;
            }
        }
        #endregion
    }
}
