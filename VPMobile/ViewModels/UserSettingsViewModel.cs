using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VP_Mobile.ViewModels
{
    public class UserSettingsViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public UserSettingsViewModel()
        {
        }
        #endregion

        #region public properties
        private MainViewModel _mainView;
        public MainViewModel MainView
        {
            get { return _mainView; }
            set
            {
                _mainView = value;
                NotifyPropertyChanged();
            }
        }

        public String UnitNumber
        {
            get
            {
                return Properties.Settings.Default.UnitNumber;
            }
            set
            {
                Properties.Settings.Default.UnitNumber = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public bool AutoRoute
        {
            get
            {
                return Properties.Settings.Default.AutoRoute;
            }
            set
            {
                Properties.Settings.Default.AutoRoute = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public bool AvlLabel
        {
            get
            {
                return Properties.Settings.Default.AvlLabel;
            }
            set
            {
                Properties.Settings.Default.AvlLabel = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public bool GpsLocLabel
        {
            get
            {
                return Properties.Settings.Default.GpsLocLabel;
            }
            set
            {
                Properties.Settings.Default.GpsLocLabel = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public bool GpsKeepNorth
        {
            get
            {
                return Properties.Settings.Default.GpsKeepNorth;
            }
            set
            {
                Properties.Settings.Default.GpsKeepNorth = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public bool IncidentLabel
        {
            get
            {
                return Properties.Settings.Default.IncidentLabel;
            }
            set
            {
                Properties.Settings.Default.IncidentLabel = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public int IncidentZoomWidth
        {
            get
            {
                return Properties.Settings.Default.IncidentZoomWidth;
            }
            set
            {
                Properties.Settings.Default.IncidentZoomWidth = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public int IncidentRefreshInterval
        {
            get
            {
                return Properties.Settings.Default.IncidentRefreshInterval;
            }
            set
            {
                Properties.Settings.Default.IncidentRefreshInterval = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public int IncidentRefreshIntervalInSeconds
        {
            get
            {
                return Properties.Settings.Default.IncidentRefreshInterval / 1000;
            }
            set
            {
                Properties.Settings.Default.IncidentRefreshInterval = value * 1000;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
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
        #endregion
    }
}
