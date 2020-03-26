using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VP_Mobile.Models
{
    class IntegerValidationRule : ValidationRule
    {
        private int _minimumValue = 0;
        private int _maximumValue = 0;
        private string _errorMessage;

        public int MinimumValue
        {
            get { return _minimumValue; }
            set { _minimumValue = value; }
        }

        public int MaximumValue
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
            var result = new ValidationResult(true, null);
            var inputString = (value ?? string.Empty).ToString();
            int tmpInt;
            if (!int.TryParse(inputString, out tmpInt) || tmpInt > MaximumValue || tmpInt < MinimumValue)
                result = new ValidationResult(false, ErrorMessage);
            return result;
        }
    }
}
