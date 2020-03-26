using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VPMobileAdmin.Converters
{
    public class UnderScoreConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String ret = value as String;
            if (value == null)
                return null;
            return ret.Replace("_", "__");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String ret = value as String;
            if (value == null)
                return null;
            return ret.Replace("__", "_");
        }
    }
}
