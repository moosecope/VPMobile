using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

namespace VPMobileRuntime100_1_0.Model
{
    public class PictureSymbol
    {
        private PictureMarkerSymbol _symbol;
        public PictureMarkerSymbol EsriSymbol
        {
            get { return _symbol; }
        }

        public static PictureSymbol FromImage(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, image.RawFormat);
                byte[] imageBytes = ms.ToArray();
                
                String imageData = Convert.ToBase64String(imageBytes);
                String jsonSymbol = GetPictureMarkerSymbolJson(imageData, GetImageMimeType(image.RawFormat), 0, image.Width, image.Height);
                return PictureSymbol.FromJson(jsonSymbol);
            }
        }

        public static PictureSymbol FromImageFile(String path)
        {
            using (Image image = Image.FromFile(path))
            {
                return PictureSymbol.FromImage(image);
            }
        }

        public static PictureSymbol FromJson(String json)
        {
            PictureSymbol ret = new PictureSymbol();
            ret._symbol = (PictureMarkerSymbol)Symbol.FromJson(json);
            return ret;
        }

        public static String GetPictureMarkerSymbolJson(String base64Data, String imageMimeType, int angle, double width, double height)
        {
            return $"{{\"type\" : \"esriPMS\", \"imageData\" : \"{base64Data}\", \"contentType\" : \"{imageMimeType}\", \"color\" : null, \"width\" : {width}, \"height\" : {height}, \"angle\" : {angle}}}";
        }

        private static string GetImageMimeType(ImageFormat format)
        {
            String mimeType = "image/unknown";

            try
            {
                Guid id = format.Guid;
                
                if (id == ImageFormat.Png.Guid)
                {
                    mimeType = "image/png";
                }
                else if (id == ImageFormat.Jpeg.Guid)
                {
                    mimeType = "image/jpeg";
                }
                else if (id == ImageFormat.Bmp.Guid)
                {
                    mimeType = "image/bmp";
                }
                else if (id == ImageFormat.Gif.Guid)
                {
                    mimeType = "image/gif";
                }
                else if (id == ImageFormat.Exif.Guid)
                {
                    mimeType = "image/jpeg";
                }
                else if (id == ImageFormat.Emf.Guid)
                {
                    mimeType = "image/x-emf";
                }
                else if (id == ImageFormat.Icon.Guid)
                {
                    mimeType = "image/ico";
                }
                else if (id == ImageFormat.MemoryBmp.Guid)
                {
                    mimeType = "image/bmp";
                }
                else if (id == ImageFormat.Tiff.Guid)
                {
                    mimeType = "image/tiff";
                }
                else if (id == ImageFormat.Wmf.Guid)
                {
                    mimeType = "image/wmf";
                }
            }
            catch
            {
            }

            return mimeType;
        }

        public String toJson()
        {
            return _symbol.ToJson();
        }
    }
}
