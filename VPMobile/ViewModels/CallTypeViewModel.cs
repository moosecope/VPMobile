using GTG.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VP_Mobile.StaticHelpers;
using VPMobileRuntime100_1_0.Model;

namespace VP_Mobile.ViewModels
{
    public class CallTypeViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public CallTypeViewModel(String callType, String callImage, int imageSize)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                    { nameof(callType), callType }
                    ,{ nameof(callImage), callImage }
                });
                var match = AvlGroupViewModel.VPImageReg.Match(callImage);
                CallTypeActive = true;
                if (!match.Success)
                    return;

                CallTypeImageJson = PictureSymbol.GetPictureMarkerSymbolJson(match.Groups[2].Value, match.Groups[1].Value, 0, imageSize, imageSize);


                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(match.Groups[2].Value)))
                {
                    var image = Image.FromStream(ms);
                    var bmp = image as Bitmap;
                    if (bmp != null)
                    {
                        _originalCallImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                      bmp.GetHbitmap(),
                                      IntPtr.Zero,
                                      Int32Rect.Empty,
                                      BitmapSizeOptions.FromEmptyOptions());
                        var gbmp = new Bitmap(bmp.Width, bmp.Height);
                        for (var i = 0; i < bmp.Width; i++)
                        {
                            for (var j = 0; j < bmp.Height; j++)
                            {
                                var originalColor = bmp.GetPixel(i, j);
                                var grayScale = (originalColor.R + originalColor.G + originalColor.B) / 3;
                                var pixelColor = System.Drawing.Color.FromArgb(originalColor.A, grayScale, grayScale, grayScale);
                                gbmp.SetPixel(i, j, pixelColor);
                            }
                        }
                        _deactivatedCallImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                      gbmp.GetHbitmap(),
                                      IntPtr.Zero,
                                      Int32Rect.Empty,
                                      BitmapSizeOptions.FromEmptyOptions());
                    }
                }

                CallTypeValue = callType;
            }
            catch (Exception ex)
            {
                var message = "Error initializing CallTypeViewModel";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion

        #region public properties
        public bool CallTypeActive
        {
            get
            {
                return !HiddenCallTypes.Contains(_callType);
            }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(value), value }
                    });
                    if (value && HiddenCallTypes.Contains(_callType))
                    {
                        HiddenCallTypes.Remove(_callType);
                    }
                    else if (!HiddenCallTypes.Contains(_callType))
                    {
                        HiddenCallTypes.Add(_callType);
                    }
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("CallTypeImage");
                }
                catch (Exception ex)
                {
                    var message = "Error setting CallTypeActive";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private static ObservableCollection<String> _hiddenCallTypes;
        public static ObservableCollection<String> HiddenCallTypes
        {
            get
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_hiddenCallTypes != null)
                        return _hiddenCallTypes;
                    var types = new ObservableCollection<String>();
                    foreach (var type in Properties.Settings.Default.HiddenCallTypes.Split(','))
                    {
                        if (!types.Contains(type))
                            types.Add(type);
                    }
                    _hiddenCallTypes = types;
                    types.CollectionChanged += HiddenTypes_CollectionChanged;
                    return _hiddenCallTypes;
                }
                catch (Exception ex)
                {
                    var message = "Error pulling HiddenCallTypes";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                    return _hiddenCallTypes;
                }
            }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_hiddenCallTypes != null)
                        _hiddenCallTypes.CollectionChanged -= HiddenTypes_CollectionChanged;
                    _hiddenCallTypes = value;
                    _hiddenCallTypes.CollectionChanged += HiddenTypes_CollectionChanged;
                    HiddenTypes_CollectionChanged(null, null);
                }
                catch (Exception ex)
                {
                    var message = "Error setting HiddenCallTypes";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private static void HiddenTypes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                Properties.Settings.Default.HiddenCallTypes = HiddenCallTypes.Aggregate((agg, cur) => agg + "," + cur);
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                var message = "Error on HiddenTypes changed";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private String _callType;
        public String CallTypeValue
        {
            get { return _callType; }
            set
            {
                _callType = value;
                NotifyPropertyChanged();
            }
        }

        private BitmapSource _originalCallImage;
        public BitmapSource OriginalCallImage
        {
            get
            {
                return _originalCallImage;
            }
        }

        private BitmapSource _deactivatedCallImage;
        public BitmapSource CallTypeImage
        {
            get
            {
                return CallTypeActive ? _originalCallImage : _deactivatedCallImage;
            }
        }

        public String CallTypeImageJson { get; set; }
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
