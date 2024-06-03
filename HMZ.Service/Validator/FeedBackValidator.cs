using System.ComponentModel.DataAnnotations;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.FeedBackServices;
namespace HMZ.Service.Validator
{
    public class FeedBackValidator : IValidator<FeedBackQuery>
    {
        private readonly IFeedBackService _feedBackService;
        public FeedBackValidator(IFeedBackService feedBackService)
        {
            _feedBackService = feedBackService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(FeedBackQuery entity, string? userName = null, bool? isUpdate = false)
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
                var feedBack = await _feedBackService.GetByIdAsync(entity.Id.ToString());
                if (feedBack.Entity == null)
                {
                    return new List<ValidationResult>(){
                        new ValidationResult("Không tìm thấy dữ liệu", new[] { nameof(entity.Id) })
                    };
                }
            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.Title), entity.Title),
                ValidatorCustom.IsRequired(nameof(entity.Description), entity.Description),
            };
            return await Task.FromResult(result);
        }
    }
}