
using System.ComponentModel.DataAnnotations;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.ClassServices;
using HMZ.Service.Services.CourseServices;

namespace HMZ.Service.Validator
{
    public class ClassValidator : IValidator<ClassQuery>
    {
        private readonly IClassService _classService;
        private readonly ICourseService _courseService;
        public ClassValidator(IClassService classService, ICourseService courseService)
        {
            _classService = classService;
            _courseService = courseService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(ClassQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
            var course = await _courseService.GetByIdAsync(entity.CourseId.ToString());
            if (course.Entity == null)
            {
                return new List<ValidationResult>(){
                        new ValidationResult("Vui lòng chọn đúng khóa học", new[] { nameof(entity.CourseId) })
                    };
            }

            // update
            if (isUpdate == true)
            {
                var getClass = await _classService.GetByIdAsync(entity.Id.ToString());
                if (getClass.Entity == null)
                {
                    return new List<ValidationResult>(){
                        new ValidationResult("Không tìm thấy lớp học", new[] { nameof(entity.Id) })
                    };
                }

            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.Name), entity.Name),
                ValidatorCustom.IsRequired(nameof(entity.CourseId), entity.CourseId),
            };
            return await Task.FromResult(result);
        }
    }
}