using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.ClassServices;
using HMZ.Service.Services.LearningProcessServices;
using HMZ.Service.Services.RoomServices;
using HMZ.Service.Services.ScheduleDetailServices;
using HMZ.Service.Services.ScheduleServices;
using NPOI.SS.UserModel;

namespace HMZ.Service.Validator
{
    public class LearningProcessValidator : IValidator<LearningProcessQuery>
    {
        private readonly ILearningProcessService _learningProcessService;
        private readonly IScheduleService   _scheduleService;
        private readonly IScheduleDetailService   _scheduleDetailService;
        public  LearningProcessValidator(ILearningProcessService learningProcessService, IScheduleService scheduleService,IScheduleDetailService scheduleDetailService)
        {
            _learningProcessService = learningProcessService;
            _scheduleService = scheduleService;
            _scheduleDetailService = scheduleDetailService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(LearningProcessQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
         
            var schedule = await _scheduleService.GetByIdAsync(entity.ScheduleId.ToString());
            if (schedule.Entity == null)
            {
                return new List<ValidationResult>(){
                        new ValidationResult("Vui lòng chọn lịch dạy", new[] { nameof(entity.ScheduleId) })
                    };
            }
            var scheduleDetail = await _scheduleDetailService.GetByIdAsync(entity.ScheduleDetailId.ToString());
            if (scheduleDetail.Entity == null)
            {
                return new List<ValidationResult>(){
                        new ValidationResult("Vui lòng chọn lịch dạy chi tiết", new[] { nameof(entity.ScheduleDetailId) })
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
            };
            return await Task.FromResult(result);
        }
    }
}