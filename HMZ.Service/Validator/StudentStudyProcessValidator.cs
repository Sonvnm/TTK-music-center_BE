
using System.ComponentModel.DataAnnotations;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.LearningProcessServices;
using HMZ.Service.Services.UserServices;

namespace HMZ.Service.Validator
{
    public class StudentStudyProcessValidator : IValidator<StudentStudyProcessQuery>
    {
        private readonly ILearningProcessService _learningProcessService;
        private readonly IUserService _userService;
        
        public StudentStudyProcessValidator(ILearningProcessService learningProcessService, IUserService userService)
        {
            _learningProcessService = learningProcessService;
            _userService = userService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(StudentStudyProcessQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
            var learningProcess = await _learningProcessService.GetByIdAsync(entity.LearningProcessId.ToString());
            if (learningProcess.Entity == null)
            {
                return new List<ValidationResult>(){
                        new ValidationResult("Không tìm thấy quá dạy học", new[] { nameof(entity.LearningProcessId) })
                    };
            }

            var user = await _userService.GetByIdAsync(entity.UserId.ToString());
            if (user.Entity == null)
            {
                return new List<ValidationResult>(){
                        new ValidationResult("Không tìm thấy sinh viên", new[] { nameof(entity.UserId) })
                    };
            }

            // update
            if (isUpdate == true)
            {
                var getClass = await _learningProcessService.GetByIdAsync(entity.Id.ToString());
                if (getClass.Entity == null)
                {
                    return new List<ValidationResult>(){
                        new ValidationResult("Không tìm thấy lớp học", new[] { nameof(entity.Id) })
                    };
                }
            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.IsAbsent), entity.IsAbsent),
                ValidatorCustom.IsRequired(nameof(entity.IsLate), entity.IsLate),
            };
            return await Task.FromResult(result);
        }
    }
}