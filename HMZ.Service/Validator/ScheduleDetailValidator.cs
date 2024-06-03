
using System.ComponentModel.DataAnnotations;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;

namespace HMZ.Service.Validator
{
    public class ScheduleDetailValidator : IValidator<ScheduleDetailQuery>
    {
        public async Task<List<ValidationResult>> ValidateAsync(ScheduleDetailQuery entity, string? userName = null, bool? isUpdate = false)
        {
            List<ValidationResult> result = new List<ValidationResult>();

            if (entity == null)
            {
                result.Add(new ValidationResult("Entity is null", new[] { nameof(entity) }));
                return result;
            }
            result.AddRange(new[] {
                ValidatorCustom.IsRequired(nameof(entity.ScheduleId), entity.ScheduleId)
            });
            return await Task.FromResult(result);
        }
    }
}