using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using GTG.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VP_Mobile.StaticHelpers;

namespace VP_Mobile.ViewModels
{
    public class AvlViewModel : Dictionary<String, Object>
    {
        #region public
        #region public constructor
        public AvlViewModel(Dictionary<string, Object> properties) : base(properties)
        {
        }

        public AvlViewModel(Getavllastreportrecorddataresult arec)
        {
            // Required Fields
            this.Add(GROUP_ID, arec.GroupID);
            this.Add(UNIT_ID, arec.UnitID);
            this.Add(UNIT_ALIAS, arec.UnitAlias);
            this.Add(LATITUDE, arec.Lat);
            this.Add(LONGITUDE, arec.Lon);

            // Additional Fields
            this.Add("GroupName", arec.GroupName);
        }
        #endregion

        #region public properties

        public int GroupID
        {
            get
            {
                object ret;
                if (base.TryGetValue(GROUP_ID, out ret))
                    return Convert.ToInt32(ret);
                else
                    return -1;
            }
        }

        public String UnitLabel
        {
            get
            {
                String ret = UnitAlias;
                if (ret == null || ret.Length == 0)
                {
                    ret = UnitID;
                }
                return ret;
            }
        }

        public String UnitID
        {
            get
            {
                object ret;
                if (base.TryGetValue(UNIT_ID, out ret))
                    return ret?.ToString();
                else
                    return String.Empty;
            }
        }

        public String UnitAlias
        {
            get
            {
                object ret;
                if (base.TryGetValue(UNIT_ALIAS, out ret))
                    return ret?.ToString();
                else
                    return String.Empty;
            }
        }

        public double Latitude
        {
            get
            {
                object ret;
                if (base.TryGetValue(LATITUDE, out ret))
                    return Convert.ToDouble(ret);
                else
                    return 0.0;
            }
        }

        public double Longitude
        {
            get
            {
                object ret;
                if (base.TryGetValue(LONGITUDE, out ret))
                    return Convert.ToDouble(ret);
                else
                    return 0.0;
            }
        }

        private Graphic _graphic;
        public Graphic Graphic
        {
            get
            {

                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_graphic != null)
                        return _graphic;
                    _graphic = new Graphic(new MapPoint(Longitude, Latitude, SpatialReferences.Wgs84), this);
                    return _graphic;
                }
                catch (Exception ex)
                {
                    var message = "Error creating graphic";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                    return _graphic;
                }
            }
        }

        public BitmapImage GroupImage { get; internal set; }

        #endregion

        #region public events
        #endregion
        #region public methods
        #endregion
        #endregion

        #region private
        public const String GPS_SYMBOL_JSON = "{\"type\" : \"esriPMS\", \"imageData\" : \"iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwwAADsMBx2+oZAAAABl0RVh0U29mdHdhcmUAUGFpbnQuTkVUIHYzLjUuNUmK/OAAAAToSURBVFhHrVdrTJtVGMZN3aIz8dcyL5lRpzEmZG7GZdMlJi4EjQacl81CaQeMy7h2DDYKUqClUCDdJpdOYcVoCgMK2ZSuICwbLjYxOIFlCQRiIoNt/5YsRuOS/Xh838Pp19Z+o+XyJE++c95z3ud7er5zTs+JiQQADcQ5ImznzgnWdXWhltnZGSDVOc7tEheJGcSdUideCC4HlPQuEdlVVUK8vqcHTRcuoG1wEB0jI+j1+QQ7hodFjNu4T0NvLyqdTk6FlGKtN2RRHdThEVkU4DoLMIrq6+GdmMDY/DzGb9/G77duCV5bWBD017lt7OZNDN+4AXt3t8zGc0TW2iKlWXurLAZAwQ2yKMCdiEjQ63GGftVvJMwviGSAOXHnDoYnJ5FRXs4SjB1SVoDqIT82DNRhHbGUMw+mp6PN7V62gcs0CrllZSzBeElKq4M6hDmi2A7O1GZmrtiAoaKCJRjbiBuldDio8f/fn0cgnYhDOTkrNlBsNrMEgw1skvLhoMaHjsCBVXyCQlo9ErulbHSgBJ61eZypWcUnOFZTwxKMWCkdHSiBP4GFM7VHjggDV6Zn0egeQfHXfUixnEVcoR1xBQFqKVZ8pg+Oi1fxyx9/CgOldXUswdglpdVBHdTmgEBWSQkqHU68k3cSryaZo+Le/FNo81yCkfaPICx7EsI7Ngbz6dP4WH8YewzN2GVwRMW3j7YgQZeGY5WV6PR6WYoRNs8UUOOjsihA9fWcYaQh/CQ5GZ9l5KPA0oDPG3oQX+tekh/V9eK4vQXZ5Rbs12iQe+IESylbsiqo/TFZ9L9cLEFGUlYW3j9sxL6sarSf/wEu3zj6x6fhmZ7D9F9/Y/affzE4M4/z12fR/et1uH66hMSSk9CZmlFstUqVyAaU4eGyZA4RSdnZwsDLB00o6PBgdGoK1+g/YeruXczdv4+FBw8wc+8eJmj2+2ZnYXT/jO2ZdmGgpLaWJRgaKR8Cii++lwtKRYLqG4hIoVXgN7C/ZTCiAe33PsVAqc3GEoy9UpZ118liiAEe9uBR2Eh8hbhWBp6R0oGXEpQyFZ4QBQmq8yp4mrgmBqSsAFVVDSiT0A+KbSKu1Qg8JWXVQR0UV35Q7FnO5K14pQaCVsFbUjZ6UNLjnPnFKgwYqqtZgqE2wmE/OgTU4UXO1OfnKwbMnQMRDdh/vKIY+NJuZwmG2ghHNPAhZ+rz8pBpasSnBjMaXW5UdA3B0ncZZ0fH4aXNaGhmAc6rk7D2j6KqexjN/R7oKk7B/G3fkgYeCn9nem7mTD6Q1DkcOGqxYXdBE97M/WpJ7ilsRpm9CZ1DQygJHEiWZUAcTun5AmdqaSvmv+NYfQ1eS7FGxdhD1sUjmcnEEoz1QjwIFFM3RQ1ip6Ln4iSUJyKtxYmtiaVRMdX2nTCQazSyBCOZGGKC6sqOGAJq2CyffI6DTo4An4haPaMoau1BwvEm7MtrwHu5AXKM21oHRpUTUUGpOFgznhTiQaBYxBH4gDNT6c9opUeyIAPbhHgQKLb0vKAOr3OmJi1txQYyDAaWYDwvZaMHJcVxZmJKCppcrmUbGKDTVGpREUswEqVs9KCk+MVc0D5gEpfTNlpagnQRZX5Dxy0//TFu5wuqqb1dZguIOwE9o1+ODEo4wNlswEqjEHw9V67okiIm2210MQ0yoJNyKoiJ+Q8vKUUw8KR8SgAAAABJRU5ErkJggg==\", \"contentType\" : \"png\", \"color\" : null, \"width\" : 30, \"height\" : 30, \"angle\" : 0}";
        public const String GROUP_ID = "GroupID";
        public const String UNIT_ID = "UnitID";
        public const String UNIT_LABEL = "UnitLabel";
        public const String UNIT_ALIAS = "UnitAlias";
        public const String LATITUDE = "Lat";
        public const String LONGITUDE = "Lon";
        #endregion
    }
}
