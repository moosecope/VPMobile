using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VP_Mobile.Models
{
    public class DoubleValidationRule : ValidationRule
    {
        private double _minimumValue = 0;
        private double _maximumValue = 0;
        private string _errorMessage;

        public double MinimumValue
        {
            get { return _minimumValue; }
            set { _minimumValue = value; }
        }

        public double MaximumValue
        {
            get { return _maximumValue; }
            set { _maximumValue = value; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var inputString = (value ?? string.Empty).ToString();
            Double tmpDbl = 0;
            if (!Double.TryParse(inputString, out tmpDbl) || tmpDbl >= MaximumValue || tmpDbl < MinimumValue)
                return new ValidationResult(false, ErrorMessage);
            return new ValidationResult(true, null);
        }
    }
}
