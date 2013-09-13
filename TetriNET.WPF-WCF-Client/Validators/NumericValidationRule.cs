using System;
using System.Globalization;
using System.Windows.Controls;

namespace TetriNET.WPF_WCF_Client.Validators
{
    public class NumericValidationRule : ValidationRule
    {
        public bool PositiveOnly { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public NumericValidationRule()
        {
            PositiveOnly = false;
            Minimum = int.MinValue;
            Maximum = int.MaxValue;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string inputString = (value ?? String.Empty).ToString();
            int inputInt;
            if (!int.TryParse(inputString, out inputInt))
                return new ValidationResult(false, "A positive integer is required");
            if (PositiveOnly && inputInt < 0)
                return new ValidationResult(false, "A positive integer is required");
            if (inputInt < Minimum)
                return new ValidationResult(false, String.Format("Must be greater than {0}", Minimum));
            if (inputInt > Maximum)
                return new ValidationResult(false, String.Format("Must be smaller than {0}", Maximum));
            return new ValidationResult(true, null);
        }
    }
}
