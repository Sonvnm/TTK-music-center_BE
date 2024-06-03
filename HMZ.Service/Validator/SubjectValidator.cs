using System.ComponentModel.DataAnnotations;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.SubjectServices;
namespace HMZ.Service.Validator
{
    public class SubjectValidator : IValidator<SubjectQuery>
    {
        private readonly ISubjectService _subjectService;
        public SubjectValidator(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(SubjectQuery entity, string? userName = null, bool? isUpdate = false)
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
                var room = await _subjectService.GetByIdAsync(entity.Id.ToString());
                if (room.Entity == null)
                {
                    return new List<ValidationResult>(){
                        new ValidationResult("Không tìm thấy môn học", new[] { nameof(entity.Id) })
                    };
                }
            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.Name), entity.Name)
            };
            return await Task.FromResult(result);
        }
    }
}