using HMZ.Database.Commons;
using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.SDK.Extensions;
using HMZ.Service.Extensions;
using HMZ.Service.Helpers;
using HMZ.Service.MailServices;
using HMZ.Service.Services.IBaseService;
using HMZ.Service.Validator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace HMZ.Service.Services.ScheduleServices
{
    public class ScheduleService : ServiceBase<IUnitOfWork>, IScheduleService
    {
        private readonly IValidator<ScheduleQuery> _validator;
        private readonly UserManager<User> _userManager;
        private readonly IMailService _mailService; 
        public ScheduleService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider, IValidator<ScheduleQuery> validator, IMailService mailService,UserManager<User> userManager) : base(unitOfWork, serviceProvider)
        {
            _validator = validator;
            _mailService = mailService;
            _userManager = userManager;
        }

        public async Task<DataResult<bool>> CreateAsync(ScheduleQuery entity)
        {

            var result = new DataResult<bool>();

            List<ValidationResult> validationResult = new List<ValidationResult>();

            if (_validator != null)
            {
                validationResult = await _validator.ValidateAsync(entity);

                if (validationResult.HasError())
                {
                    result.Errors.AddRange(validationResult.JoinError());
                    return result;
                }
            }

            var courseEntity = await _unitOfWork.GetRepository<Course>()
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.Id.Equals(entity.CourseId));

            var classExist = await _unitOfWork.GetRepository<Schedule>()
                .AsQueryable().Include(x => x.Class)
                .Where(x => x.ClassId.Equals(entity.ClassId)).FirstOrDefaultAsync();
            if (classExist != null)
            {
                result.Errors.Add($"Lớp học {classExist.Class.Name} đã tồn tại lịch học");
            }
            if (courseEntity.StartDate.Value > entity.StartDate)
            {
                result.Errors.Add($"Lịch dạy phải bắt đầu sau ngày bắt đầu của khóa học {courseEntity.StartDate.Value.Date}");
            }

            if (courseEntity.EndDate.Value < entity.EndDate)
            {
                result.Errors.Add($"Lịch dạy phải kết thúc trước ngày kết thúc của khóa học {courseEntity.EndDate.Value.Date}");
            }
            var schedules = await _unitOfWork.GetRepository<Schedule>().AsQueryable().ToListAsync();

            if (schedules.Any(x => x.CourseId == entity.CourseId && x.Name.Equals(entity.Name.Trim())))
            {
                result.Errors.Add($"Tên lịch học đã tồn tại trong khóa học");

            }
            if (result.Errors.Any())
            {
                return result;
            }



            var schedule = new Schedule
            {
                Name = entity.Name.Trim(),
                Code = HMZHelper.GenerateCode(10, "SCH"),
                Description = entity.Description,
                CourseId = entity.CourseId,
                ClassId = entity.ClassId,
                StartDate = entity.StartDate.Value.Date,
                EndDate = entity.EndDate.Value.Date,
                Status = EScheduleStatus.Accepted,
                CreatedBy = entity.CreatedBy
            };
            await _unitOfWork.GetRepository<Schedule>().Add(schedule);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.EntityId = schedule.Id.ToString();
                result.Message = "Tạo lịch dạy thành công";
            }

            return result;
        }

        public async Task<DataResult<int>> DeleteAsync(string[] id)
        {
            var result = new DataResult<int>();
            var scheduleRepository = _unitOfWork.GetRepository<Schedule>();
            var courseRepository = _unitOfWork.GetRepository<Course>();

            if (id == null || id.Length == 0)
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }

            var schedules = await scheduleRepository.AsQueryable()
                .Where(x => id.Contains(x.Id.ToString()))
                .ToListAsync();
            if (!schedules.Any())
            {
                result.Errors.Add("Lịch dạy không tồn tại");
            }

            if (schedules.Where(x => x.StartDate.Value.Date <= DateTime.Now.Date).Any())
            {
                result.Errors.Add("Chỉ được xóa lịch dạy chưa bắt đầu");
                return result;
            }
            // remove all leaning progress by schedule
            var leaningProgressRepository = _unitOfWork.GetRepository<LearningProcess>();
            var scheduleDetailRes = _unitOfWork.GetRepository<ScheduleDetail>();
            var schedulesDetail = await scheduleDetailRes.AsQueryable()
               .Where(x => id.Contains(x.ScheduleId.ToString()))
               .ToListAsync();
            var leaningProgress = await leaningProgressRepository.AsQueryable()
                .Where(x => id.Contains(x.ScheduleId.ToString()))
                .ToListAsync();
            var studentClassRepository = _unitOfWork.GetRepository<StudentStudyProcess>();
            var studentClass = await studentClassRepository.AsQueryable()
                .Where(x => leaningProgress.Select(l => l.Id).Contains(x.LearningProcessId))
                .ToListAsync();

            if (studentClass.Any() || leaningProgress.Any())
            {
                result.Errors.Add("Vui lòng xóa học viên trước khi xóa lịch dạy và Lịch điểm danh");
                return result;
            }
            if (schedulesDetail.Any())
            {
                scheduleDetailRes.DeleteRange(schedulesDetail);

            }
            scheduleRepository.DeleteRange(schedules);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa lịch dạy thành công !";
                return result;
            }
            result.Errors.Add("Xóa lịch dạy thất bại");
            return result;
        }

        public Task<DataResult<ScheduleView>> GetByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public async Task<DataResult<ScheduleView>> GetByIdAsync(string id)
        {
            var result = new DataResult<ScheduleView>();
            var schedule = await _unitOfWork.GetRepository<Schedule>()
                .AsQueryable()
                .Include(x => x.Course)
                .Where(x => x.Id.ToString() == id)
                .Select(x => new ScheduleView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    Course = x.Course,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status.ToString()

                }).FirstOrDefaultAsync();
            if (schedule == null)
            {
                result.Errors.Add("Không tim thầy lịch dạy này !");
                return result;
            }
            result.Entity = schedule;
            return result;
        }



        public async Task<DataResult<ScheduleView>> GetPageList(BaseQuery<ScheduleFilter> query)
        {
            var result = new DataResult<ScheduleView>();
            if (query != null)
            {
                var courseName = query?.Entity?.CourseName;
                var className = query?.Entity?.ClassName;
                if (query.Entity != null)
                {
                    query.Entity.CourseName = null;
                    query.Entity.ClassName = null;
                }
                var listSchedule = await _unitOfWork.GetRepository<Schedule>().AsQueryable().Include(x => x.Class)
                    .Include(x => x.Course)
                        .Where(x =>
              (string.IsNullOrEmpty(courseName) || x.Course.Name.Contains(courseName)) &&
              (string.IsNullOrEmpty(className) || x.Class.Name.Contains(className)))
                    .ApplyFilter(query)
                    .OrderByColumns(query.SortColumns, query.SortOrder)
                    .ToListAsync();
                if (listSchedule.Any())
                {
                    result.TotalRecords = listSchedule.Count();

                    result.Items = listSchedule.Skip((query.PageNumber.Value - 1) * query.PageSize.Value).Take(query.PageSize.Value).Select(
                        x => new ScheduleView(x)
                        {
                            Name = x.Name,
                            Description = x.Description,
                            Course = x.Course,
                            CourseId = x.CourseId,
                            StartDate = x.StartDate,
                            EndDate = x.EndDate,
                            Status = x.Status.ToString(),
                            ClassName = x.Class?.Name??"",
                            ClassId = x.ClassId!=null?x.ClassId:null
                        }).ToList();
                }
            }
            return result;
        }

        public async Task<DataResult<ScheduleView>> GetSchedulesByUser(string userName)
        {
            var result = new DataResult<ScheduleView>();
            var user = await _unitOfWork.GetRepository<Database.Entities.User>().AsQueryable().FirstOrDefaultAsync(x => x.UserName == userName);

            var classes = await _unitOfWork.GetRepository<Class>().AsQueryable()
                .Include(x => x.Course).Include(x => x.StudentClasses).ThenInclude(x => x.User).Where(x => x.StudentClasses.Any(y => y.UserId == user.Id)).ToListAsync();



            if (!classes.Any())
            {
                result.Errors.Add("Chưa tồn tại lớp  học nào cả");
                return result;
            }
            var schedules = await _unitOfWork.GetRepository<Schedule>().AsQueryable().Include(x => x.Course).Where(x => classes.Select(classe => classe.CourseId).ToList().Contains(x.CourseId)).ToListAsync();
            if (!schedules.Any())
            {
                {
                    result.Errors.Add("Không tìm thấy lịch học nào cả");
                    return result;
                }
            }

            result.Items = schedules.Where(x=>x.EndDate.Value.Date>=DateTime.Now).Select(x => new ScheduleView(x)
            {
                Name = x.Name,
                Description = x.Description,
                Course = x.Course,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status.ToString(),
                Class = x.Class
            }).ToList();
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(ScheduleQuery entity, string id)
        {
            var result = new DataResult<int>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<ScheduleQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity, isUpdate: true);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Update entity
            var scheduleRepository = _unitOfWork.GetRepository<Schedule>();
            var schedule = await scheduleRepository.AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (schedule == null)
            {
                result.Errors.Add("Không tìm thấy lịch dạy");
                return result;
            }
            var courseEntity = await _unitOfWork.GetRepository<Course>()
              .AsQueryable()
              .FirstOrDefaultAsync(x => x.Id.Equals(entity.CourseId));
            if (courseEntity.StartDate.Value > entity.StartDate)
            {
                result.Errors.Add($"Lịch dạy phải bắt đầu sau ngày bắt đầu của khóa học {courseEntity.StartDate.Value.Date}");
            }

            if (courseEntity.EndDate.Value < entity.EndDate)
            {
                result.Errors.Add($"Lịch dạy phải kết thúc trước ngày kết thúc của khóa học {courseEntity.EndDate.Value.Date}");
            }
            var schedules = await _unitOfWork.GetRepository<Schedule>().AsQueryable().ToListAsync();
            if (schedules.Any(x => x.CourseId == entity.CourseId && x.Name.Equals(entity.Name.Trim()) && !schedule.Name.Equals(entity.Name.Trim())))
            {
                result.Errors.Add($"Tên lịch học đã tồn tại trong khóa học");

            }
            if (result.Errors.Any())
            {
                return result;
            }
            schedule.Name = entity.Name;
            schedule.Description = entity.Description;
            schedule.StartDate = entity.StartDate;
            schedule.EndDate = entity.EndDate;
            schedule.UpdatedBy=entity.UpdatedBy;

            schedule.UpdatedAt = DateTime.Now;
            scheduleRepository.Update(schedule);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Cập nhật lịch dạy thành công";
                result.EntityId = schedule.Id.ToString();
                return result;
            }
            result.Errors.Add("Cập nhật lịch dạy thất bại");
            return result;
        }
        public async Task<DataResult<bool>> SendMail(string scheduleId,Guid scheduleDetailId,Guid? roomId)
        {
            var result = new DataResult<bool>();

            var schedule = await _unitOfWork.GetRepository<Schedule>().AsQueryable().Where(x => x.Id.Equals(Guid.Parse(scheduleId))).Include(x => x.Class).ThenInclude(x => x.Course).FirstOrDefaultAsync();
            var scheduleDetail = await _unitOfWork.GetRepository<ScheduleDetail>().AsQueryable().Where(x => x.Id.Equals(scheduleDetailId)).FirstOrDefaultAsync();
            var studentClass = await _unitOfWork.GetRepository<StudentClass>().AsQueryable().Where(x => x.ClassId.Equals(schedule.ClassId)).Include(x => x.User).ToListAsync();
            var room = await _unitOfWork.GetRepository<Room>().AsQueryable().Where(x => x.Id.Equals(roomId)).FirstOrDefaultAsync();
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "EmailTemplates");
            templatePath = Path.Combine(templatePath, "ScheduleMail.html");
            var newLearningProcess = new LearningProcess
            {
                ScheduleDetailId = scheduleDetail.Id,
                ScheduleId = schedule.Id,
                Assets = "",
                Description = "Lịch học bù",
                UserId = studentClass.FirstOrDefault(x => x.Role == EPersonRoles.Teacher).UserId,

                CreatedBy = studentClass.FirstOrDefault(x => x.Role == EPersonRoles.Teacher).User.UserName,
            };
            _unitOfWork.GetRepository<LearningProcess>().Add(newLearningProcess);
            await _unitOfWork.SaveChangesAsync();
            var htmlTemplate = "";
            if (System.IO.File.Exists(templatePath))
            {
                htmlTemplate = System.IO.File.ReadAllText(templatePath);

            }


            htmlTemplate = htmlTemplate.Replace("{courseName}", schedule.Class.Course.Name);
            htmlTemplate = htmlTemplate.Replace("{startDateCourse}", String.Format("{0:dd/MM/yyyy}", schedule.Class.Course.StartDate));
            htmlTemplate = htmlTemplate.Replace("{endDateCourse}", String.Format("{0:dd/MM/yyyy}", schedule.Class.Course.EndDate));
            htmlTemplate = htmlTemplate.Replace("{startDate}", String.Format("{0:dd/MM/yyyy HH:mm}", scheduleDetail.StartTime.Value));
           /* htmlTemplate = htmlTemplate.Replace("{startDateSchedule}", String.Format("{0:dd/MM/yyyy}", schedule.StartDate));*/
       /*     htmlTemplate = htmlTemplate.Replace("{endDateSchedule}", String.Format("{0:dd/MM/yyyy}", schedule.EndDate));*/
            htmlTemplate = htmlTemplate.Replace("{className}", schedule.Class.Name);
            htmlTemplate = htmlTemplate.Replace("{scheduleDescription}", schedule.Description);
            htmlTemplate = htmlTemplate.Replace("{scheduleName}", scheduleDetail.Name);
            htmlTemplate = htmlTemplate.Replace("{roomName}", room.Name);
            htmlTemplate = htmlTemplate.Replace("{teacherName}", studentClass.FirstOrDefault(x => x.Role == EPersonRoles.Teacher).User.FirstName + " " + studentClass.FirstOrDefault(x => x.Role == EPersonRoles.Teacher).User.LastName);


            if (studentClass.Any())
            {
                var emails = studentClass.Select(x => x.User.Email).ToList();
                var mail = new MailQuery()
                {
                    Subject = $"[Thông báo] Lịch học cho khóa {schedule.Class.Course.Name}",
                    Body = htmlTemplate,
                    ToEmails = emails
                };
                result = await _mailService.SendEmailAsync(mail);
            }
            return result;


        }
    }
}
