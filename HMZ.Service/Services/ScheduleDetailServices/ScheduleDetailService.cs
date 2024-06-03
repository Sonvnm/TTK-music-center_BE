
using System.ComponentModel.DataAnnotations;
using HMZ.Database.Entities;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Extensions;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;
using HMZ.Service.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HMZ.SDK.Extensions;
using HMZ.Database.Commons;
using NPOI.SS.Formula.Functions;
using HMZ.Database.Enums;
using HMZ.Service.Services.ScheduleServices;

namespace HMZ.Service.Services.ScheduleDetailServices
{
    public class ScheduleDetailService : ServiceBase<IUnitOfWork>, IScheduleDetailService
    {
        private readonly IScheduleService _scheduleService;
        public ScheduleDetailService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider, IScheduleService scheduleService) : base(unitOfWork, serviceProvider)
        {
            this._scheduleService = scheduleService;
        }

        public async Task<DataResult<bool>> CreateAsync(ScheduleDetailQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<ScheduleDetailQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // check startDate in Schedule start and end date
            var schedule = await _unitOfWork.GetRepository<Schedule>().AsQueryable().Include(x => x.Class).ThenInclude(x => x.StudentClasses)
                .Where(x => x.Id == entity.ScheduleId && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (entity.IsMakeUpClass == true && !schedule.Class.StudentClasses.Any(x => x.Role.Equals(EPersonRoles.Student)))
            {
                result.Errors.Add("Lớp học chưa tồn tại thành viên nào ");
                return result;
            }
            if (schedule == null)
            {
                result.Errors.Add("Không tìm thấy lịch học");
                return result;
            }
            if (entity.IsMakeUpClass != true)
            {
                if (entity.StartDate.Value.Date < schedule.StartDate.Value.Date || entity.StartDate.Value.Date > schedule.EndDate.Value.Date)
                {
                    result.Errors.Add($"Ngày học phải trong khoảng từ {schedule.StartDate.Value.ToString("dd/MM/yyyy")} đến {schedule.EndDate.Value.ToString("dd/MM/yyyy")} ");
                    return result;
                }
                if (entity.StartDate.Value.Date < DateTime.Now.Date)
                {
                    result.Errors.Add($"Ngày học phải lớn hơn hoặc bằng hôm nay");
                    return result;
                }
            }

            if (entity.IsMakeUpClass == true && entity.StartDate.Value.Date < DateTime.Now.Date)
            {
                result.Errors.Add($"Ngày học phải lớn hơn hoặc bằng hôm nay");
                return result;
            }
            // check room is available in Time
            var dateNow = DateTime.Now;
            var scheduleDetails = await _unitOfWork.GetRepository<ScheduleDetail>().AsQueryable()
                .Where(x => x.IsActive == true)
                .ToListAsync();

            var startTime = entity.StartDate.Value.Date;
            var endTime = entity.StartDate.Value.Date;
            switch (entity.TimeSlot)
            {
                case 1:// 7h-9h
                    startTime = startTime.AddHours(7);
                    endTime = endTime.AddHours(9);
                    break;
                case 2:// 13h-15h
                    startTime = startTime.AddHours(13);
                    endTime = endTime.AddHours(15);
                    break;
                case 3:// 18h-20h
                    startTime = startTime.AddHours(18);
                    endTime = endTime.AddHours(20);
                    break;
            }
            if (scheduleDetails != null && scheduleDetails.Count > 0)
            {
                var isExist = scheduleDetails.Any(x => x.StartTime.Value == startTime && x.EndTime.Value == endTime && x.RoomId.Equals(entity.RoomId));
                if (isExist)
                {
                    result.Errors.Add("Phòng học đã được sử dụng");
                    return result;
                }

                var isExistSchedule = scheduleDetails.Any(x => x.StartTime.Value == startTime && x.EndTime.Value == endTime && x.ScheduleId.Equals(entity.ScheduleId));
                if (isExistSchedule)
                {
                    result.Errors.Add("Ca học này đã được tạo !");
                    return result;
                }
            }
            // Create entity
            var scheduleDetail = new ScheduleDetail
            {
                Name = entity.Name,
                RoomId = entity.RoomId,
                ScheduleId = entity.ScheduleId,
                StartTime = startTime,
                EndTime = endTime,
                Code = HMZHelper.GenerateCode(8, "SD"),
                IsMakeUpClass = entity.IsMakeUpClass

            };
            await _unitOfWork.GetRepository<ScheduleDetail>().Add(scheduleDetail);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                if (entity.IsMakeUpClass == true)
                {
                    var check = await _scheduleService.SendMail(scheduleDetail.ScheduleId.ToString(), (Guid)scheduleDetail.Id, entity.RoomId);
                    
                    if (check.Success == true)
                    {
                        result.Message = "Tạo lịch bù thành công ! Thư điện thử đã được gửi cho học viên !";
                    }

                }
                else
                {
                    result.Message = "Tạo thành công";
                }

                return result;
            }
            return result;
        }




        public async Task<DataResult<int>> DeleteAsync(string[] id)
        {
            var result = new DataResult<int>();
            if (id == null || id.Length == 0)
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var repository = _unitOfWork.GetRepository<ScheduleDetail>();
            var scheduleDetails = await repository.AsQueryable()
                .Where(x => id.Contains(x.Id.ToString()) && x.IsActive == true)
                .ToListAsync();

            if (scheduleDetails == null || scheduleDetails.Count == 0)
            {
                result.Errors.Add("Không tìm thấy chi tiết lịch học");
                return result;
            }
            var learingProcess = await _unitOfWork.GetRepository<LearningProcess>().AsQueryable().Where(x => id.Contains(x.ScheduleDetailId.ToString())).ToListAsync();
            if (learingProcess.Any())
            {
                foreach (var item in learingProcess)
                {
                    if (item.Status.Equals(ELearningProcessStatus.Done))
                    {
                        var scheduleDetail = scheduleDetails.FirstOrDefault(x => x.Id.Equals(item.ScheduleDetailId));
                        result.Errors.Add($"Không thể xóa lịch dạy chi tiết {scheduleDetail.Name} đã được hoàn thành !");
                        return result;
                    }
                }
                _unitOfWork.GetRepository<LearningProcess>().DeleteRange(learingProcess);

            }

            repository.DeleteRange(scheduleDetails);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa chi tiết lịch học thành công";
                return result;
            }
            result.Errors.Add("Xóa chi tiết lịch học thất bại");
            return result;
        }

        public async Task<DataResult<ScheduleDetailView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<ScheduleDetailView>();
            if (string.IsNullOrEmpty(code))
            {
                result.Errors.Add("Code is null or empty");
                return result;
            }
            var scheduleDetail = await _unitOfWork.GetRepository<ScheduleDetail>().AsQueryable()
                .Include(x => x.Schedule)
                .Include(x => x.Room)
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new ScheduleDetailView(x)
                {
                    Name = x.Name,
                    RoomName = x.Room.Name,
                    RoomId = x.RoomId,
                    ScheduleId = x.ScheduleId,
                    ScheduleName = x.Schedule.Name,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    IsMakeUpClass = x.IsMakeUpClass
                })
                .FirstOrDefaultAsync();
            if (scheduleDetail == null)
            {
                result.Errors.Add("Không tìm thấy phòng học");
                return result;
            }
            result.Entity = scheduleDetail;
            return result;
        }

        public async Task<DataResult<ScheduleDetailView>> GetByIdAsync(string id)
        {
            var result = new DataResult<ScheduleDetailView>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var scheduleDetail = await _unitOfWork.GetRepository<ScheduleDetail>().AsQueryable()
                .Include(x => x.Schedule)
                .Include(x => x.Room)
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .Select(x => new ScheduleDetailView(x)
                {
                    Name = x.Name,
                    ScheduleName = x.Schedule.Name,
                    ScheduleId = x.ScheduleId,
                    RoomName = x.Room.Name,
                    RoomId = x.RoomId,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    IsMakeUpClass = x.IsMakeUpClass
                })
                .FirstOrDefaultAsync();
            if (scheduleDetail == null)
            {
                result.Errors.Add("Không tìm thấy chi tiết lịch học");
                return result;
            }
            result.Entity = scheduleDetail;
            return result;
        }

        public async Task<DataResult<ScheduleDetailView>> GetPageList(BaseQuery<ScheduleDetailFilter> query)
        {
            var result = new DataResult<ScheduleDetailView>();
            string roomName = query.Entity?.RoomName;
            if (query?.Entity != null)
            {
                query.Entity.RoomName = null;
            }
            var repository = _unitOfWork.GetRepository<ScheduleDetail>().AsQueryable()
                .Include(x => x.Schedule)
                .Include(x => x.Room)
                .ApplyFilter(query)
                .OrderByColumns(query.SortColumns, query.SortOrder);

            result.TotalRecords = await repository.CountAsync();
            result.Items = await repository
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new ScheduleDetailView(x)
                    {
                        Name = x.Name,
                        ScheduleName = x.Schedule.Name,
                        RoomName = x.Room.Name,
                        RoomId = x.RoomId,
                        ScheduleId = x.ScheduleId,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime,
                        IsMakeUpClass = x.IsMakeUpClass
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<ScheduleDetailView>> GetSchedulesDetailByClassId(Guid classId)
        {
            var result = new DataResult<ScheduleDetailView>();

            var scheduleDetail = await _unitOfWork.GetRepository<ScheduleDetail>().AsQueryable()
                .Include(x => x.Schedule)
                .Include(x => x.Room)
                .Where(x => x.Schedule.ClassId == classId && x.IsActive == true)
                .Select(x => new ScheduleDetailView(x)
                {
                    Name = x.Name,
                    RoomName = x.Room.Name,
                    RoomId = x.RoomId,
                    ScheduleId = x.ScheduleId,
                    ScheduleName = x.Schedule.Name,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    IsMakeUpClass = x.IsMakeUpClass
                })
                .ToListAsync();
            if (scheduleDetail == null)
            {
                result.Errors.Add("Chưa có lịch dạy chi tiết");
                return result;
            }
            result.Items = scheduleDetail;
            return result;
        }

        public async Task<DataResult<ScheduleDetailView>> GetSchedulesDetailByScheduleId(string scheduleId)
        {
            var result = new DataResult<ScheduleDetailView>();

            var scheduleDetail = await _unitOfWork.GetRepository<ScheduleDetail>().AsQueryable()
                .Include(x => x.Schedule).ThenInclude(x => x.Class)
                .Include(x => x.Room).OrderBy(x => x.StartTime).Where(x => x.EndTime.Value.Date >= DateTime.Now.Date).ToListAsync();

            var data = scheduleDetail.Where(x => x.ScheduleId.Equals(Guid.Parse(scheduleId)))
            .Select(x => new ScheduleDetailView(x)
            {
                Name = x.Name,
                RoomName = x.Room.Name,
                RoomId = x.RoomId,
                ScheduleId = x.ScheduleId,
                ScheduleName = x.Schedule.Name,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                ClassName = x.Schedule.Class.Name,
                IsMakeUpClass = x.IsMakeUpClass
            }).ToList();
            if (data == null)
            {
                result.Errors.Add("Hiện chưa có lịch dạy chi tiết");
                return result;
            }
            result.Items = data;
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(ScheduleDetailQuery entity, string id)
        {
            var result = new DataResult<int>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<ScheduleDetailQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // check startDate in Schedule start and end date
            var schedule = await _unitOfWork.GetRepository<Schedule>().AsQueryable()
                .Where(x => x.Id == entity.ScheduleId && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (schedule == null)
            {
                result.Errors.Add("Không tìm thấy lịch học");
                return result;
            }
            if (entity.IsMakeUpClass != true)
            {
                if (entity.StartDate.Value.Date < schedule.StartDate.Value.Date || entity.StartDate.Value.Date > schedule.EndDate.Value.Date)
                {
                    result.Errors.Add($"Ngày học phải trong khoảng từ {schedule.StartDate.Value.ToString("dd/MM/yyyy")} đến {schedule.EndDate.Value.ToString("dd/MM/yyyy")} ");
                    return result;
                }
                if (entity.StartDate.Value.Date < DateTime.Now.Date)
                {
                    result.Errors.Add($"Ngày học phải lớn hơn hoặc bằng hôm nay");
                    return result;
                }
            }

            if (entity.IsMakeUpClass == true && entity.StartDate.Value.Date < DateTime.Now.Date)
            {
                result.Errors.Add($"Ngày học phải lớn hơn hoặc bằng hôm nay");
                return result;
            }
            // check room is available in Time
            var dateNow = DateTime.Now;
            var scheduleDetails = await _unitOfWork.GetRepository<ScheduleDetail>().AsQueryable()
                .Where(x => x.IsActive == true)
                .ToListAsync();

            var myScheduleDetail = scheduleDetails.Where(x => x.Id.Equals(entity.ScheduleDetailId)).FirstOrDefault();

            var startTime = entity.StartDate.Value.Date;
            var endTime = entity.StartDate.Value.Date;
            switch (entity.TimeSlot)
            {
                case 1:// 7h-9h
                    startTime = startTime.AddHours(7);
                    endTime = endTime.AddHours(9);
                    break;
                case 2:// 13h-15h
                    startTime = startTime.AddHours(13);
                    endTime = endTime.AddHours(15);
                    break;
                case 3:// 18h-20h
                    startTime = startTime.AddHours(18);
                    endTime = endTime.AddHours(20);
                    break;
            }
            if (scheduleDetails.Any())
            {
                var isExist = scheduleDetails.Any(x => x.StartTime.Value == startTime && x.EndTime.Value == endTime && x.RoomId.Equals(entity.RoomId) && !x.Id.Equals(myScheduleDetail.Id));
                if (isExist)
                {
                    result.Errors.Add("Phòng học đã được sử dụng");
                    return result;
                }

                var isExistSchedule = scheduleDetails.Any(x => x.StartTime.Value == startTime && x.EndTime.Value == endTime && x.ScheduleId.Equals(entity.ScheduleId) && !x.Id.Equals(myScheduleDetail.Id));
                if (isExistSchedule)
                {
                    result.Errors.Add("Ca học này đã được tạo !");
                    return result;
                }
            }
            // Create entity
            myScheduleDetail.Name = entity.Name;
            myScheduleDetail.StartTime = startTime;
            myScheduleDetail.EndTime = endTime;
            myScheduleDetail.RoomId = entity.RoomId;
            myScheduleDetail.IsMakeUpClass = entity.IsMakeUpClass;
            myScheduleDetail.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<ScheduleDetail>().Update(myScheduleDetail);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                return result;
            }
            result.Errors.Add("Sửa chi tiết lịch học thất bại");
            return result;
        }
    }
}