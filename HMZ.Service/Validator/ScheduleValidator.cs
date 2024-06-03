
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using System.ComponentModel.DataAnnotations;


namespace HMZ.Service.Validator
{
    public class ScheduleValidator : IValidator<ScheduleQuery>
    {


        public async Task<List<ValidationResult>> ValidateAsync(ScheduleQuery entity, string? userName = null, bool? isUpdate = false)
        {
            List<ValidationResult> result = new List<ValidationResult>();

            if (entity == null)
            {
                result.Add(new ValidationResult("Entity is null", new[] { nameof(entity) }));
                return result;

            }

            result.AddRange(new[] {
                ValidatorCustom.IsRequired(nameof(entity.Name), entity.Name),
                ValidatorCustom.IsRequired(nameof(entity.StartDate), entity.StartDate),
                ValidatorCustom.IsRequired(nameof(entity.EndDate), entity.EndDate),
                ValidatorCustom.IsRequired(nameof(entity.CourseId), entity.CourseId),
                ValidatorCustom.MinDate(nameof(entity.StartDate),entity.StartDate.Value.Date,DateTime.Now.Date, $"Ngày bắt đầu {entity.StartDate.Value.Date} không thể nhỏ hơn ngày hiện tại"),
                ValidatorCustom.MinDate(nameof(entity.EndDate), entity.EndDate.Value.Date, entity.StartDate.Value.Date, $"Ngày kết thúc {entity.EndDate.Value.Date} không thể nhỏ hơn ngày bắt đầu {entity.StartDate.Value.Date}")
            });
            return await Task.FromResult(result);
        }
    }
}
