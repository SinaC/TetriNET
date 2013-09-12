using System;
using System.Globalization;
using System.Windows.Controls;

namespace TetriNET.WPF_WCF_Client.Validators
{
    public class NumericValidationRule : ValidationRule
    {
        public bool PositiveOnly { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string inputString = (value ?? String.Empty).ToString();
            int inputInt;
            if (!int.TryParse(inputString, out inputInt))
                return new ValidationResult(false, "A positive integer is required");
            if (PositiveOnly && inputInt < 0)
                return new ValidationResult(false, "A positive integer is required");
            return new ValidationResult(true, null);
        }
    }
}
