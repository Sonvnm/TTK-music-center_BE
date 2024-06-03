using System.ComponentModel.DataAnnotations;

namespace HMZ.Service.Extensions
{
    public static class ValidatorExtension
    {
        public static List<ValidationResult> AddErrors(this List<ValidationResult> results, IEnumerable<ValidationResult> errors)
        {
            if (errors != null)
            {
                results.AddRange(errors.Where(x => x != ValidationResult.Success));
            }
            return results;
        }
        public static List<ValidationResult> AddError(this List<ValidationResult> results, ValidationResult error)
        {
            if (error != null && error != ValidationResult.Success)
            {
                results.Add(error);
            }
            return results;
        }
        // is Error
        public static bool IsError(this ValidationResult results)
        => results != ValidationResult.Success;
        // has Error
        public static bool HasError(this List<ValidationResult> results)
        => results != null && results.Any(x => x.IsError());
        // join Error
        public static string[] JoinError(this List<ValidationResult> results)
        {
            //results.Where(x => x != ValidationResult.Success).Select(x => x.ErrorMessage)
            return results.Where(x => x.IsError()).Select(x => x.ErrorMessage).ToArray();
        }
    }
}