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
using HMZ.Database.Enums;

namespace HMZ.Service.Services.LearningProcessServices
{
    public class LearningProcessService : ServiceBase<IUnitOfWork>, ILearningProcessService
    {
        public LearningProcessService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(unitOfWork, serviceProvider)
        {
        }

        public async Task<DataResult<bool>> CreateAsync(LearningProcessQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<LearningProcessQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            var schedulesDetail = await _unitOfWork.GetRepository<ScheduleDetail>().AsQueryable().Include(x => x.Schedule).ThenInclude(x => x.Class).ThenInclude(x => x.StudentClasses).ToListAsync();

            var scheduleDetail = schedulesDetail.Where(x => x.Id.Equals(entity.ScheduleDetailId)).FirstOrDefault();


            var learningProcess = await _unitOfWork.GetRepository<LearningProcess>().AsQueryable().Include(x => x.ScheduleDetail).ToListAsync();
            var duplicateTableTime = learningProcess.Any(x => x.UserId.Equals(entity.UserId) && x.ScheduleDetail.StartTime.Equals(scheduleDetail.StartTime) && x.ScheduleDetail.EndTime.Equals(scheduleDetail.EndTime));
            if (duplicateTableTime)
            {
                result.Errors.Add("Trùng thời gian ở lịch dạy chi tiết khác");
                return result;
            }
            var learningProcessExist = learningProcess.FirstOrDefault(x => x.ScheduleDetailId.Equals(entity.ScheduleDetailId));
            if (learningProcessExist != null)
            {
                result.Errors.Add("Lịch dạy chi tiết này đã được đăng ký");
                return result;
            }
           /* if (DateTime.Now.CompareTo(scheduleDetail.StartTime) >= 0)
            {
                result.Errors.Add("Quá thời gian tạo lịch học ở tiết này");
                return result;
            }*/
            // Create entity
            var newLearningProcess = new LearningProcess
            {
                Name = entity.Name,
                ScheduleDetailId = entity.ScheduleDetailId,
                ScheduleId = entity.ScheduleId,
                Assets = entity.Assets,
                Description = entity.Description,
                UserId = entity.UserId,

                CreatedBy = entity.CreatedBy,
            };

            if (scheduleDetail.Schedule.Class == null)
            {
                result.Errors.Add("Chưa tồn tại lớp học nào cả");
                return result;
            }
            var studentClasses = scheduleDetail.Schedule.Class.StudentClasses.Where(x => !x.UserId.Equals(entity.UserId));
            if (!studentClasses.Any())
            {
                result.Errors.Add("Chưa tồn tại học viên nào cả");
                return result;
            }

            await _unitOfWork.GetRepository<LearningProcess>().Add(newLearningProcess);

            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Đã thêm thành công";
                return result;
            }
            result.Errors.Add("Lỗi khi thêm mới");
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
            var learningProcessRepository = _unitOfWork.GetRepository<LearningProcess>();
            var learningProcess = await learningProcessRepository.AsQueryable().Include(x => x.ScheduleDetail)
                .Where(x => id.Contains(x.Id.ToString()))
                .ToListAsync();

            if (learningProcess == null || learningProcess.Count == 0)
            {
                result.Errors.Add("Không tìm thấy Tiến trình học tập");
                return result;
            }
            foreach (var process in learningProcess)
            {
                if (DateTime.Now.CompareTo(process.ScheduleDetail.EndTime) >= 0 && process.Status.Equals(ELearningProcessStatus.Done))
                {
                        result.Errors.Add("Không thể xóa tiến trình đã được kết thúc");
                        return result;
                }
               
            }

            // check studentprcocess
            var studentProcessRepository = _unitOfWork.GetRepository<StudentStudyProcess>();
            var studentProcess = await studentProcessRepository.AsQueryable()
                .Where(x => id.Contains(x.LearningProcessId.ToString()))
                .ToListAsync();
          


            studentProcessRepository.DeleteRange(studentProcess);
            learningProcessRepository.DeleteRange(learningProcess); // false: remove from db, true: set IsActive = false
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa tiến trình học tập thành công";
                return result;
            }
            result.Errors.Add("Xóa tiến trình học tập thất bại");
            return result;
        }

        public async Task<DataResult<LearningProcessView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<LearningProcessView>();
            if (string.IsNullOrEmpty(code))
            {
                result.Errors.Add("Code is null or empty");
                return result;
            }
            var learningProcessView = await _unitOfWork.GetRepository<LearningProcess>().AsQueryable()
                .Include(x => x.User)
                .Include(x => x.ScheduleDetail).ThenInclude(x => x.Schedule)
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new LearningProcessView(x)
                {
                    Name = x.Name,
                    Assets = x.Assets,
                    Description = x.Description,
                    StartTime = x.ScheduleDetail.StartTime,
                    EndTime = x.ScheduleDetail.EndTime,
                    ScheduleCode = x.ScheduleDetail.Schedule.Code,
                    Username = x.User.UserName,
                })
                .FirstOrDefaultAsync();
            if (learningProcessView == null)
            {
                result.Errors.Add("Không tìm thấy tiến trình học tập");
                return result;
            }
            result.Entity = learningProcessView;
            return result;
        }

        public async Task<DataResult<LearningProcessView>> GetByIdAsync(string id)
        {
            var result = new DataResult<LearningProcessView>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var learningProcessView = await _unitOfWork.GetRepository<LearningProcess>().AsQueryable()
                .Include(x => x.User)
                .Include(x => x.ScheduleDetail).ThenInclude(x => x.Schedule)
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .Select(x => new LearningProcessView(x)
                {
                    Name = x.Name,
                    Assets = x.Assets,
                    Description = x.Description,
                    StartTime = x.ScheduleDetail.StartTime,
                    EndTime = x.ScheduleDetail.EndTime,

                    ScheduleCode = x.ScheduleDetail.Schedule.Code,
                    ScheduleName = x.ScheduleDetail.Schedule.Name,
                    Username = x.User.UserName,
                })
                .FirstOrDefaultAsync();
            if (learningProcessView == null)
            {
                result.Errors.Add("Không tìm thấy tiến trình học tập");
                return result;
            }
            result.Entity = learningProcessView;
            return result;
        }
        public async Task<DataResult<LearningProcessView>> GetByUsername(string userName)
        {
            var result = new DataResult<LearningProcessView>();

            var startDate=new DateTime(DateTime.Now.Year,DateTime.Now.Month,1);
            var endDate= new DateTime(DateTime.Now.Year, DateTime.Now.Month+1, 1);

            var learningProcessView = await _unitOfWork.GetRepository<LearningProcess>().AsQueryable()
                .Include(x => x.User)
                .Include(x => x.ScheduleDetail).ThenInclude(x => x.Schedule).ThenInclude(x=>x.Class).ThenInclude(x => x.Course)
                .Where(x => x.CreatedBy.Equals(userName) && x.ScheduleDetail.StartTime.Value.Date >= startDate.Date && x.ScheduleDetail.EndTime.Value.Date<=endDate.Date && x.Status.Equals(ELearningProcessStatus.Done) && x.ScheduleDetail.IsMakeUpClass!=true) 
                .Select(x => new LearningProcessView(x)
                {
                    Name = x.Name,
                    Assets = x.Assets,
                    Description = x.Description,
                    ScheduleDetail = x.ScheduleDetail,
                    ScheduleName = x.ScheduleDetail.Name,
                    Username = x.User.UserName,
                    Status = x.Status.ToString(),
                    StartTime = x.ScheduleDetail.StartTime,
                    EndTime = x.ScheduleDetail.EndTime,
                    ClassName=x.ScheduleDetail.Schedule.Class.Name,
                    CourseName=x.ScheduleDetail.Schedule.Course.Name

                })
                .ToListAsync();
            if (learningProcessView == null)
            {
                result.Errors.Add("Không tìm thấy tiến trình học tập");
                return result;
            }
            result.Items = learningProcessView;
            return result;
        }
        public async Task<DataResult<LearningProcessView>> GetByUser(string userName)
        {
            var result = new DataResult<LearningProcessView>();


            var learningProcessView = await _unitOfWork.GetRepository<LearningProcess>().AsQueryable()
                .Include(x => x.User)
                .Include(x => x.ScheduleDetail).ThenInclude(x => x.Room).ThenInclude(x => x.Schedules).ThenInclude(x => x.Course)
                .Where(x => x.CreatedBy.Equals(userName))
                .Select(x => new LearningProcessView(x)
                {
                    Name = x.Name,
                    Assets = x.Assets,
                    Description = x.Description,
                    ScheduleDetail = x.ScheduleDetail,
                    Username = x.User.UserName,
                    Status = x.Status.ToString(),
                    StartTime=x.ScheduleDetail.StartTime,
                    EndTime=x.ScheduleDetail.EndTime,

                })
                .ToListAsync();
            if (learningProcessView == null)
            {
                result.Errors.Add("Không tìm thấy tiến trình học tập");
                return result;
            }
            result.Items = learningProcessView;
            return result;
        }

        public async Task<DataResult<ChartView>> GetDashboardData()
        {
            var result = new DataResult<ChartView>();
            var chart = new ChartView();
            var labels = new List<string>();
            var values = new List<int>();
            var dateNow = DateTime.Now;
            var startDate = new DateTime(dateNow.Year, dateNow.Month, 1); // Giới hạn từ đầu tháng
            var endDate = startDate.AddMonths(1).AddDays(-1); // Giới hạn đến cuối tháng

            var learningProcessQuery = _unitOfWork.GetRepository<LearningProcess>()
            .AsQueryable()
            .Include(x => x.ScheduleDetail);
            var learningProcess = await learningProcessQuery
                .Where(x => x.ScheduleDetail.StartTime.Value >= startDate && x.ScheduleDetail.EndTime.Value <= endDate)
                .ToListAsync();

            for (var dt = dateNow; dt <= endDate; dt = dt.AddDays(1))
            {
                var total = learningProcess.Where(x => x.ScheduleDetail.StartTime.Value != null && x.ScheduleDetail.StartTime.Value.Date == dt.Date).Count();
                labels.Add(dt.ToString("dd/MM/yyyy"));
                values.Add(total);
            }
            chart.Labels = labels;
            chart.Values = values;
            chart.Total = await learningProcessQuery.CountAsync();
            chart.TotalYesterday = await learningProcessQuery
                .Where(x => x.CreatedAt != null && x.CreatedAt.Value.Date == dateNow.AddDays(-1).Date && x.IsActive == true)
                .CountAsync();
            chart.TotalToday = await learningProcessQuery
                .Where(x => x.CreatedAt != null && x.CreatedAt.Value.Date == dateNow.Date && x.IsActive == true)
                .CountAsync();
            chart.Percentage = chart.TotalToday > 0 ? (chart.TotalToday - chart.TotalYesterday) * 100 / chart.TotalToday : 0;

            result.Entity = chart;
            return result;
        }

        public async Task<DataResult<LearningProcessView>> GetPageList(BaseQuery<LearningProcessFilter> query)
        {
            var result = new DataResult<LearningProcessView>();
            string username = query.Entity?.Username ?? string.Empty;
            string shcheduleCode = query.Entity?.ScheduleCode ?? string.Empty;
            if (query.Entity != null)
            {
                query.Entity.Username = null;
                query.Entity.ScheduleCode = null;
            }
            var user = await GetUserLoginAsync();
            // check role
            var role = await _unitOfWork.GetRepository<UserRole>().AsQueryable()
                .Include(x => x.Role)
                .Where(x => x.UserId == user.Id)
                .Select(x => x.Role.Name)
                .ToListAsync();
            bool isAdmin = role.Contains(EUserRoles.Admin.ToString());
            if (!isAdmin)
            {
                query.Entity.CreatedBy = user.UserName;
            }

            var roomQuery = _unitOfWork.GetRepository<LearningProcess>().AsQueryable()
                            .Include(x => x.User)
                            .Include(x => x.ScheduleDetail).ThenInclude(x => x.Schedule).ThenInclude(x=>x.Class).ThenInclude(x => x.Course)
                            .ApplyFilter(query)
                            .Where(x => (string.IsNullOrEmpty(username) || x.User.UserName.Contains(username))
                                && (string.IsNullOrEmpty(shcheduleCode) || x.ScheduleDetail.Schedule.Code.Contains(shcheduleCode))
                                && x.IsActive == true)
                            .OrderByColumns(query.SortColumns, query.SortOrder);

            result.TotalRecords = await roomQuery.CountAsync();
            result.Items = await roomQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new LearningProcessView(x)
                    {
                        Name = x.Name,
                        Assets = x.Assets,
                        Description = x.Description,
                        StartTime = x.ScheduleDetail.StartTime,
                        EndTime = x.ScheduleDetail.EndTime,
                        ClassId=x.ScheduleDetail.Schedule.ClassId,
                        ClassName=x.ScheduleDetail.Schedule.Class.Name,

                        ScheduleCode = x.ScheduleDetail.Schedule.Code,
                        ScheduleName = x.ScheduleDetail.Schedule.Name,
                        Username = x.User.UserName,
                        CourseName = x.ScheduleDetail.Schedule.Course.Name,
                        Status = x.Status.ToString()

                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(LearningProcessQuery entity, string id)
        {
            var result = new DataResult<int>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<LearningProcessQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity, isUpdate: true);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Update entity
            var learningProcessRepository = _unitOfWork.GetRepository<LearningProcess>();
            var learningProcess = await learningProcessRepository.AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (learningProcess == null)
            {
                result.Errors.Add("Không tìm thấy tiến trình học tập");
                return result;
            }

            learningProcess.Name = entity.Name;
            learningProcess.ScheduleDetailId = entity.ScheduleDetailId;
            learningProcess.Assets = entity.Assets;
            learningProcess.Description = entity.Description;
     /*       learningProcess.Status= (ELearningProcessStatus)entity.Status;*/

            learningProcess.UpdatedBy = entity.UpdatedBy;
            learningProcess.UpdatedAt = DateTime.Now;
            learningProcessRepository.Update(learningProcess);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Cập nhật tiến trình học tập thành công";
                return result;
            }
            result.Errors.Add("Cập nhật tiến trình học tập thất bại");
            return result;
        }
    }
}