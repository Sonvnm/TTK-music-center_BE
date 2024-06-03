
using System.ComponentModel.DataAnnotations;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.HistorySystemServices;
namespace HMZ.Service.Validator
{
    public class HistorySystemValidator : IValidator<HistorySystemQuery>
    {
        private readonly IHistorySystemService _historySystemService;
        public HistorySystemValidator(IHistorySystemService historySystemService)
        {
            _historySystemService = historySystemService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(HistorySystemQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
            // update
            if (isUpdate == true)
            {
                var data = await _historySystemService.GetByIdAsync(entity.Id.ToString());
                if (data.Entity == null)
                {
                    return new List<ValidationResult>(){
                        new ValidationResult("Không tìm thấy lịch sử", new[] { nameof(entity.Id) })
                    };
                }
            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.Action), entity.Action),
                ValidatorCustom.IsRequired(nameof(entity.Description), entity.Description),
            };
            return await Task.FromResult(result);
        }
    }
}