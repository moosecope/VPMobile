using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace VPMobileAdmin.Converters
{
    public class ArcgisBase64ToImageConverter : IValueConverter
    {
        private static Regex _reg;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String base64 = value as String;
            return ArcgisBase64ToImage(base64);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Image input = value as Image;
            return ImageToArcgisBase64(input);
        }

        public static string GetMimeType(Image image)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.First(codec => codec.FormatID == image.RawFormat.Guid).MimeType;
        }

        public static String ImageToArcgisBase64(Image image)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, image.RawFormat);
            byte[] imageBytes = stream.ToArray();
            return "data:" + GetMimeType(image) + ";base64," + System.Convert.ToBase64String(imageBytes) + "";
        }

        public static Image ArcgisBase64ToImage(String base64)
        {
            if (_reg == null)
                _reg = new Regex(";base64,(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var match = _reg.Match(base64);
            if (!match.Success)
                return null;
            var image = match.Groups[1].Value;
            Byte[] bitmapData = System.Convert.FromBase64String(image);
            MemoryStream streamBitmap = new MemoryStream(bitmapData);
            return Image.FromStream(streamBitmap);
        }
    }
}
