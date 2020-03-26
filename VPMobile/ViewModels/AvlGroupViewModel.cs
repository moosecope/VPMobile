using GTG.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VP_Mobile.StaticHelpers;
using VPMobileRuntime100_1_0.Model;

namespace VP_Mobile.ViewModels
{
    public class AvlGroupViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public AvlGroupViewModel(String groupName, int groupId, String groupColor, String groupImage, int imageSize)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                    { nameof(groupName), groupName }
                    , { nameof(groupId), groupId }
                    , { nameof(groupColor), groupColor }
                    , { nameof(groupImage), groupImage }
                    , { nameof(imageSize), imageSize }
                });
                Visible = true;
                Expanded = true;
                GroupName = groupName;
                GroupID = groupId;
                var colors = groupColor.Split(',');
                GroupColor = new SolidColorBrush(Color.FromArgb(byte.Parse(colors[3]), byte.Parse(colors[0]), byte.Parse(colors[1]), byte.Parse(colors[2])));

                var match = VPImageReg.Match(groupImage);
                if (!match.Success)
                    return;

                GroupImageJson = PictureSymbol.GetPictureMarkerSymbolJson(match.Groups[2].Value, match.Groups[1].Value, 0, imageSize, imageSize);
                var test = match.Groups[1].Value;

                BitmapImage bi = new BitmapImage();

                bi.BeginInit();
                bi.StreamSource = new MemoryStream(System.Convert.FromBase64String(match.Groups[2].Value));
                bi.EndInit();

                GroupImage = bi;
                _avlUnits = new ObservableCollection<AvlViewModel>();
            }
            catch (Exception ex)
            {
                var message = "Error initializing Avl Group View Model";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion

        #region public properties

        public int GroupID { get; set; }
        public Brush GroupColor { get; set; }
        public String GroupName { get; set; }
        public String GroupImageJson { get; set; }
        public BitmapImage GroupImage { get; set; }

        private static Regex _vpImageReg;
        public static Regex VPImageReg
        {
            get
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_vpImageReg == null)
                        _vpImageReg = new Regex("data:([^;]*);base64,(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    return _vpImageReg;
                }
                catch (Exception ex)
                {
                    var message = "Error initializing Avl Group View Model";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                    return null;
                }
            }
        }

        private bool _visible;
        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                NotifyPropertyChanged();
            }
        }

        private bool _expanded;
        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                _expanded = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<AvlViewModel> _avlUnits;
        public ObservableCollection<AvlViewModel> AvlUnits
        {
            get { return _avlUnits; }
            set
            {
                _avlUnits = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #endregion

        #region private

        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
