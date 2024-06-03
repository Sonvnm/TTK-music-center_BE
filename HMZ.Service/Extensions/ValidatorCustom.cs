using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace HMZ.Service.Extensions
{
    public static class ValidatorCustom
    {
        public static ValidationResult? IsRequired(string name, object value)
        {
            if (value == null
             || (value is string && string.IsNullOrWhiteSpace(value.ToString())
             || (value is IEnumerable && !(value as IEnumerable).GetEnumerator().MoveNext())

            ))
                return new ValidationResult($"{name} không được để trống");
            return null;
        }
        // min length string
        public static ValidationResult? MinLength(string name, string? value, int min)
        {
            if (value.Length < min)
                return new ValidationResult($"{name} must be greater than or equal to {min} characters");
            return null;
        }
        // max length string
        public static ValidationResult? MaxLength(string name, string? value, int max)
        {
            if (value.Length > max)
                return new ValidationResult($"{name} must be less than or equal to {max} characters");
            return null;
        }
        // email
        public static ValidationResult? Email(string name, string? value)
        {
            if (!new EmailAddressAttribute().IsValid(value))
                return new ValidationResult($"{name} is not a valid email address");
            return null;
        }
        // password
        public static ValidationResult? Password(string name, string? value)
        {
            if (!new RegularExpressionAttribute(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$").IsValid(value))
                return new ValidationResult($"{name} must contain at least 1 uppercase, 1 lowercase, 1 number, 1 special character and must be between 8 and 15 characters");
            return null;
        }
        // confirm password
        public static ValidationResult? ConfirmPassword(string name, string? value, string? confirmValue)
        {
            if (value != confirmValue)
                return new ValidationResult($"{name} không đúng !");
            return null;
        }
        // phone
        public static ValidationResult? Phone(string name, string? value)
        {
            if (!new PhoneAttribute().IsValid(value))
                return new ValidationResult($"{name} is not a valid phone number");
            return null;
        }
        // has Special Character
        public static ValidationResult? HasSpecialCharacter(string name, string? value)
        {
            if (!new RegularExpressionAttribute(@"^[a-zA-Z0-9]*$").IsValid(value))
                return new ValidationResult($"{name} must not contain special characters");
            return null;
        }
        // min int, double, float, decimal
        public static ValidationResult? Min(string name, object? value, object min)
        {
            if (value is int && (int)value < (int)min)
                return new ValidationResult($"{name} must be greater than or equal to {min}");
            if (value is double && (double)value < (double)min)
                return new ValidationResult($"{name} must be greater than or equal to {min}");
            if (value is float && (float)value < (float)min)
                return new ValidationResult($"{name} must be greater than or equal to {min}");
            if (value is decimal && (decimal)value < (decimal)min)
                return new ValidationResult($"{name} must be greater than or equal to {min}");
            return null;
        }
        // max int, double, float, decimal
        public static ValidationResult? Max(string name, object? value, object max)
        {
            if (value is int && (int)value > (int)max)
                return new ValidationResult($"{name} must be less than or equal to {max}");
            if (value is double && (double)value > (double)max)
                return new ValidationResult($"{name} must be less than or equal to {max}");
            if (value is float && (float)value > (float)max)
                return new ValidationResult($"{name} must be less than or equal to {max}");
            if (value is decimal && (decimal)value > (decimal)max)
                return new ValidationResult($"{name} must be less than or equal to {max}");
            return null;
        }
        // range int, double, float, decimal
        public static ValidationResult? Range(string name, object? value, object min, object max)
        {
            if (value is int && ((int)value < (int)min || (int)value > (int)max))
                return new ValidationResult($"{name} must be between {min} and {max}");
            if (value is double && ((double)value < (double)min || (double)value > (double)max))
                return new ValidationResult($"{name} must be between {min} and {max}");
            if (value is float && ((float)value < (float)min || (float)value > (float)max))
                return new ValidationResult($"{name} must be between {min} and {max}");
            if (value is decimal && ((decimal)value < (decimal)min || (decimal)value > (decimal)max))
                return new ValidationResult($"{name} must be between {min} and {max}");
            return null;
        }

        // min date
        public static ValidationResult? MinDate(string name, object? value, object min,string? message = null)
        {
            if (value is System.DateTime && (System.DateTime)value < (System.DateTime)min)
                return new ValidationResult(message ?? $"{name} phải lớn hơn hoặc bằng {min}");
            return null;
        }
        // max date
        public static ValidationResult? MaxDate(string name, object? value, object max, string? message = null)
        {
            if (value is System.DateTime && (System.DateTime)value > (System.DateTime)max)
                return new ValidationResult(message ?? $"{name} must be less than or equal to {max}");
            return null;
        }
        // Age
        public static ValidationResult? Age(string name, object? value, object min, object max, string? message = null)
        {
            if (value is System.DateTime && ((System.DateTime)value < System.DateTime.Now.AddYears(-(int)min) || (System.DateTime)value > System.DateTime.Now.AddYears(-(int)max)))
                return new ValidationResult(message ?? $"{name} must be between {min} and {max} years old");
            return null;
        }
    }
}