using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.CourseServices;
using HMZ.Service.Services.SubjectServices;

namespace HMZ.Service.Validator
{
    public class CourseValidator : IValidator<CourseQuery>
    {
        private readonly ICourseService _courseService;
        public CourseValidator(ICourseService courseService)
        {
            _courseService = courseService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(CourseQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
            var result = new List<ValidationResult>();
            {
                // update
                if (isUpdate == true)
                {

                    result = new List<ValidationResult>()
                    {
                        ValidatorCustom.IsRequired(nameof(entity.Name), entity.Name),
                        ValidatorCustom.IsRequired(nameof(entity.Description), entity.Description),
                        ValidatorCustom.IsRequired(nameof(entity.Price), entity.Price),
                        ValidatorCustom.Min(nameof(entity.Price), (int)entity.Price, 0),
                        ValidatorCustom.Max(nameof(entity.Price), (int)entity.Price, 1000000000),
                        ValidatorCustom.IsRequired(nameof(entity.StartDate), entity.StartDate),
                        ValidatorCustom.IsRequired(nameof(entity.EndDate), entity.EndDate),
                        ValidatorCustom.MinDate("Ngày kết thúc", entity.EndDate.Value.Date, entity.StartDate.Value.Date)
                    };
                }
                else
                {
                    result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.Name), entity.Name),
                ValidatorCustom.IsRequired(nameof(entity.Description), entity.Description),
                ValidatorCustom.IsRequired(nameof(entity.Price), entity.Price),
                ValidatorCustom.Min(nameof(entity.Price), (int)entity.Price, 0),
                ValidatorCustom.Max(nameof(entity.Price), (int)entity.Price, 1000000000),
                ValidatorCustom.IsRequired(nameof(entity.StartDate), entity.StartDate),
                ValidatorCustom.IsRequired(nameof(entity.EndDate), entity.EndDate),
                ValidatorCustom.MinDate("Ngày bắt đầu",entity.StartDate.Value.Date, DateTime.Now.Date),
                ValidatorCustom.MinDate("Ngày kết thúc", entity.EndDate.Value.Date, entity.StartDate.Value.Date),
                    };
                }


                return await Task.FromResult(result);
            }
        }
    }
}