using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VP_Mobile.Converters
{
    public class UnderScoreConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String ret = value as String;
            if (value == null)
                return null;
            return ret.Replace("_", " ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String ret = value as String;
            if (value == null)
                return null;
            return ret.Replace(" ", "_");
        }
    }
}
