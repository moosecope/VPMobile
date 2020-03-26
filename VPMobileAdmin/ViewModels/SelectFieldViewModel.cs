using ArcGisServiceInfo.ServicesInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VPMobileAdmin.ViewModels
{
    public class SelectFieldViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public SelectFieldViewModel()
        {
        }
        #endregion

        #region public properties

        private Layer _layer;
        public Layer Layer
        {
            get { return _layer; }
            set
            {
                _layer = value;
                NotifyPropertyChanged();
            }
        }

        private ArcGisServiceInfo.LayerInfo.Field _field;
        public ArcGisServiceInfo.LayerInfo.Field Field
        {
            get { return _field; }
            set
            {
                _field = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region public methods
        public bool IsValid()
        {
            return Field != null;
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
        #endregion
    }
}
