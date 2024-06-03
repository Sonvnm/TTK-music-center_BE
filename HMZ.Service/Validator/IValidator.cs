using System.ComponentModel.DataAnnotations;

namespace HMZ.Service.Validator
{
    public interface IValidator<T>
    {
        Task<List<ValidationResult>> ValidateAsync(T entity, string? userName = null, bool? isUpdate = false);
    }
}