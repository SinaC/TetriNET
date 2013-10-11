using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace TetriNET.WPF_WCF_Client.Validators
{
    public class StringValidationRule : ValidationRule
    {
        public string FieldName { get; set; }
        public bool NullAccepted { get; set; }

        public StringValidationRule()
        {
            FieldName = "Field";
            NullAccepted = false;
        }

        #region Overrides of ValidationRule

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string inputString = (value ?? String.Empty).ToString();

            if (String.IsNullOrWhiteSpace(inputString))
            {
                if (NullAccepted)
                    return new ValidationResult(true, null);
                return new ValidationResult(false, FieldName + " cannot be empty");
            }
            if (inputString.Length < 3)
                return new ValidationResult(false, FieldName + " must be between 3 and 20 characters long");
            if (Regex.IsMatch(inputString, @"\s"))
                return new ValidationResult(false, FieldName + " cannot contain whitespace");
            if (!inputString.All(c => Char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                return new ValidationResult(false, FieldName + " cannot contain special character except - and _");
            if (inputString.Count(Char.IsDigit) >= inputString.Count(c => !Char.IsDigit(c)))
                return new ValidationResult(false, FieldName + " cannot contain more digits than letters");
            return new ValidationResult(true, null);
        }

        #endregion
    }
}