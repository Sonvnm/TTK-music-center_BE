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

namespace HMZ.Service.Services.StudentStudyProcessServices
{
    public class StudentStudyProcessService : ServiceBase<IUnitOfWork>, IStudentStudyProcessService
    {
        public StudentStudyProcessService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(unitOfWork, serviceProvider)
        {

        }


        public async Task<DataResult<bool>> CreateAsync(StudentStudyProcessQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<StudentStudyProcessQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Create entity
            var studentStudyProcess = new StudentStudyProcess
            {
                IsAbsent = entity.IsAbsent,
                IsLate = entity.IsLate,
                LearningProcessId = entity.LearningProcessId,
                UserId = entity.UserId,

                Description = entity.Description,
                CreatedBy = entity.CreatedBy,
            };
            await _unitOfWork.GetRepository<StudentStudyProcess>().Add(studentStudyProcess);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Đã thêm mới thành công";
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
            var learningProcessRepository = _unitOfWork.GetRepository<StudentStudyProcess>();
            var learningProcess = await learningProcessRepository.AsQueryable()
                .Where(x => id.Contains(x.Id.ToString()))
                .ToListAsync();

            if (learningProcess == null || learningProcess.Count == 0)
            {
                result.Errors.Add("Không tìm thấy qúa trình học tập");
                return result;
            }
            learningProcessRepository.DeleteRange(learningProcess); // false: remove from db, true: set IsActive = false
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa quá trình học tập thành công";
                return result;
            }
            result.Errors.Add("Xóa quá trình học tập thất bại");
            return result;
        }

        public async Task<DataResult<StudentStudyProcessView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<StudentStudyProcessView>();
            if (string.IsNullOrEmpty(code))
            {
                result.Errors.Add("Code is null or empty");
                return result;
            }
            var studentProcessView = await _unitOfWork.GetRepository<StudentStudyProcess>().AsQueryable()
                .Include(x => x.User)
                .Include(x => x.LearningProcess)
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new StudentStudyProcessView(x)
                {
                    Description = x.Description,
                    IsAbsent = x.IsAbsent,
                    IsLate = x.IsLate,
                    LearningProcessCode = x.LearningProcess.Code,
                    Username = x.User.UserName,
                })
                .FirstOrDefaultAsync();
            if (studentProcessView == null)
            {
                result.Errors.Add("Không tìm thấy quá trình học tập");
                return result;
            }
            result.Entity = studentProcessView;
            return result;
        }

        public async Task<DataResult<StudentStudyProcessView>> GetByIdAsync(string id)
        {
            var result = new DataResult<StudentStudyProcessView>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var studentProcessView = await _unitOfWork.GetRepository<StudentStudyProcess>().AsQueryable()
                .Include(x => x.User)
                .Include(x => x.LearningProcess)
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .Select(x => new StudentStudyProcessView(x)
                {
                    Description = x.Description,
                    IsAbsent = x.IsAbsent,
                    IsLate = x.IsLate,
                    LearningProcessCode = x.LearningProcess.Code,
                    Username = x.User.UserName,
                })
                .FirstOrDefaultAsync();
            if (studentProcessView == null)
            {
                result.Errors.Add("Không tìm thấy quá trình học tập");
                return result;
            }
            result.Entity = studentProcessView;
            return result;
        }

        public async Task<DataResult<StudentStudyProcessView>> GetByLearningProcessPageList(BaseQuery<StudentStudyProcessFilter> query, Guid learningProcessId)
        {
            var result = new DataResult<StudentStudyProcessView>();
            var learningProcess = await _unitOfWork.GetRepository<LearningProcess>().AsQueryable()
                .Where(x => x.Id == learningProcessId && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (learningProcess == null)
            {
                result.Errors.Add("Không tìm thấy quá trình học tập");
                return result;
            }
            string username = query.Entity?.Username ?? string.Empty;
            string learningProcessCode = query.Entity?.LearningProcessCode ?? string.Empty;
            if (query.Entity != null)
            {
                query.Entity.Username = null;
                query.Entity.LearningProcessCode = null;
            }

            var roomQuery = _unitOfWork.GetRepository<StudentStudyProcess>().AsQueryable()
                            .Include(x => x.User)
                            .Include(x => x.LearningProcess)
                            .ApplyFilter(query)
                            .Where(x => x.LearningProcessId == learningProcessId && x.IsActive == true
                                && (string.IsNullOrEmpty(username) || x.User.UserName.Contains(username))
                                && (string.IsNullOrEmpty(learningProcessCode) || x.LearningProcess.Code.Contains(learningProcessCode)))
                            .OrderByColumns(query.SortColumns, query.SortOrder);

            result.TotalRecords = await roomQuery.CountAsync();
            result.Items = await roomQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new StudentStudyProcessView(x)
                    {
                        Description = x.Description,
                        IsAbsent = x.IsAbsent,
                        IsLate = x.IsLate,
                        LearningProcessCode = x.LearningProcess.Code,
                        LearningProcessId = x.LearningProcess.Id,
                        Username = x.User.FirstName + " " + x.User.LastName,
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<StudentStudyProcessView>> GetPageList(BaseQuery<StudentStudyProcessFilter> query)
        {
            var result = new DataResult<StudentStudyProcessView>();
            string username = query.Entity?.Username ?? string.Empty;
            string learningProcessCode = query.Entity?.LearningProcessCode ?? string.Empty;
            string learningProcessName = query.Entity?.LearningProcessName ?? string.Empty;
            if (query.Entity != null)
            {
                query.Entity.Username = null;
                query.Entity.LearningProcessCode = null;
                query.Entity.LearningProcessName = null;
            }

            var roomQuery = _unitOfWork.GetRepository<StudentStudyProcess>().AsQueryable()
                            .Include(x => x.User)
                            .Include(x => x.LearningProcess)
                            .ApplyFilter(query)
                            .Where(x => (string.IsNullOrEmpty(username) || x.User.UserName.Contains(username))
                                && (string.IsNullOrEmpty(learningProcessCode) || x.LearningProcess.Code.Contains(learningProcessCode))
                                && (string.IsNullOrEmpty(learningProcessName) || x.LearningProcess.Name.Contains(learningProcessName))
                                && x.IsActive == true)
                            .OrderByColumns(query.SortColumns, query.SortOrder);

            result.TotalRecords = await roomQuery.CountAsync();
            result.Items = await roomQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new StudentStudyProcessView(x)
                    {
                        LearningProcessName = x.LearningProcess.Name,
                        Description = x.Description,
                        IsAbsent = x.IsAbsent,
                        IsLate = x.IsLate,
                        LearningProcessCode = x.LearningProcess.Code,
                        Username = x.User.UserName,
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<StudentStudyProcessView>> GetLearningProcessByClassId(BaseQuery<StudentStudyProcessFilter> query, string classId, string userName)
        {
            var result = new DataResult<StudentStudyProcessView>();
            string learningProcessCode = query.Entity?.LearningProcessCode ?? string.Empty;
            string learningProcessName = query.Entity?.LearningProcessName ?? string.Empty;
            if (query.Entity != null)
            {
                query.Entity.LearningProcessCode = null;
                query.Entity.LearningProcessName = null;
            }

            var classRepository = await _unitOfWork.GetRepository<Class>()
                .AsQueryable()
                .Include(x => x.Course)
                .Where(x => x.Id.ToString() == classId && x.IsActive == true && x.Course.IsActive == true)
                .ToListAsync();

            var studyProcesses = _unitOfWork.GetRepository<StudentStudyProcess>().AsQueryable()
                            .Include(x => x.User)
                            .Include(x => x.LearningProcess)
                            .ThenInclude(x => x.ScheduleDetail).ThenInclude(x => x.Schedule).ThenInclude(x => x.Course)
                            .Where(x => x.User.UserName == userName && classRepository.Select(x => x.CourseId).Contains(x.LearningProcess.ScheduleDetail.Schedule.CourseId))
                            .ApplyFilter(query)
                            .Where(x =>
                                 (string.IsNullOrEmpty(learningProcessCode) || x.LearningProcess.Code.Contains(learningProcessCode))
                                && (string.IsNullOrEmpty(learningProcessName) || x.LearningProcess.Name.Contains(learningProcessName))
                                && x.IsActive == true)
                            .OrderByColumns(query.SortColumns, query.SortOrder);

            result.TotalRecords = await studyProcesses.CountAsync();
            result.Items = await studyProcesses
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new StudentStudyProcessView(x)
                    {
                        LearningProcessName = x.LearningProcess.Name,
                        Description = x.Description,
                        IsAbsent = x.IsAbsent,
                        IsLate = x.IsLate,
                        LearningProcessCode = x.LearningProcess.Code,
                        Username = x.User.UserName,
                        StartTime = x.LearningProcess.ScheduleDetail.StartTime,
                        EndTime = x.LearningProcess.ScheduleDetail.EndTime,
                        LearningProcessStatus = x.LearningProcess.Status.ToString()
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(StudentStudyProcessQuery entity, string id)
        {
            var result = new DataResult<int>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<StudentStudyProcessQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Update entity
            var studentStudyProcess = await _unitOfWork.GetRepository<StudentStudyProcess>().AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (studentStudyProcess == null)
            {
                result.Errors.Add("Không tìm thấy quá trình học tập");
                return result;
            }
            studentStudyProcess.IsAbsent = entity.IsAbsent;
            studentStudyProcess.IsLate = entity.IsLate;
            studentStudyProcess.Description = entity.Description;

            studentStudyProcess.UpdatedBy = entity.UpdatedBy;
            studentStudyProcess.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<StudentStudyProcess>().Update(studentStudyProcess);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Cập nhật quá trình học tập thành công";
                return result;
            }
            result.Errors.Add("Cập nhật quá trình học tập thất bại");
            return result;
        }

        public async Task<DataResult<bool>> UpdateProcessStudent(StudentStudyProcessQuery query, string id, string userName)
        {
            var result = new DataResult<bool>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }

            // Update entity
            var studentStudyProcess = await _unitOfWork.GetRepository<StudentStudyProcess>().AsQueryable().Include(x => x.LearningProcess).ThenInclude(x => x.ScheduleDetail)
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (studentStudyProcess == null)
            {
                result.Errors.Add("Không tìm thấy quá trình học tập");
                return result;
            }
            var deadlineEdit = studentStudyProcess.LearningProcess.ScheduleDetail.EndTime.Value.AddDays(3);
            if (DateTime.Now.Date.CompareTo(studentStudyProcess.LearningProcess.ScheduleDetail.StartTime.Value.Date) < 0)
            {
                result.Errors.Add("Chưa tới thời gian cho phép điểm danh");
                return result;
            }
            var user = await _unitOfWork.GetRepository<User>().AsQueryable().Where(x => x.UserName.Equals(userName)).Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync();

            if (DateTime.Now.Date.CompareTo(deadlineEdit.Date) >= 0 && !user.UserRoles.Select(x => x.Role.Name).ToList().Contains("Admin"))
            {
                result.Errors.Add("Quá thời gian cập nhật quá trình học tập");
                return result;
            }
            studentStudyProcess.IsAbsent = query.IsAbsent;
            studentStudyProcess.IsLate = query.IsLate;
            studentStudyProcess.Description = query.Description;

            studentStudyProcess.UpdatedBy = userName;
            studentStudyProcess.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<StudentStudyProcess>().Update(studentStudyProcess);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Cập nhật quá trình học tập thành công";
                return result;
            }
            result.Errors.Add("Cập nhật quá trình học tập thất bại");
            return result;
        }

        public async Task<DataResult<bool>> CreateStudyProcesses(List<StudentStudyProcessQuery> entity, string userName)
        {
            var result = new DataResult<bool>();
            using var scope = _serviceProvider.CreateScope();
            var learingProcessId = entity[0].LearningProcessId;
            var isUpdate = entity[0].IsUpdate;
            var learningProcess = await _unitOfWork.GetRepository<LearningProcess>().AsQueryable().Include(x => x.ScheduleDetail)
            .Where(x => x.Id.Equals(learingProcessId))
            .FirstOrDefaultAsync();
            var deadlineEdit = learningProcess.ScheduleDetail.EndTime.Value.AddDays(3);
            var learningProcessRepository = _unitOfWork.GetRepository<LearningProcess>();
            var studyProcesses = entity.Select(x =>
            {
                return new StudentStudyProcess
                {
                    IsAbsent = x.IsAbsent,
                    IsLate = x.IsLate,
                    LearningProcessId = x.LearningProcessId,
                    UserId = x.UserId,
                    Description = x.Description,
                    CreatedBy = userName,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = userName

                };
            }).ToList();
            var user = await _unitOfWork.GetRepository<User>().AsQueryable().Where(x => x.UserName.Equals(userName)).Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync();
            if (DateTime.Now.CompareTo(learningProcess.ScheduleDetail.StartTime.Value) < 0)
            {
                result.Errors.Add("Chưa tới thời gian cho phép điểm danh");
                return result;
            }

            if (!isUpdate &&DateTime.Now.Date.CompareTo(deadlineEdit.Date) >= 0 && !user.UserRoles.Select(x => x.Role.Name).ToList().Contains("Admin"))
            {
                result.Errors.Add("Quá thời gian cho phép điểm danh");
                return result;
            }
            if (isUpdate)
            {

                if (DateTime.Now.Date.CompareTo(deadlineEdit.Date) >= 0 && !user.UserRoles.Select(x => x.Role.Name).ToList().Contains("Admin"))
                {
                    result.Errors.Add("Quá thời gian cập nhật điểm danh");
                    return result;
                }
                var updateStudyProcess = await _unitOfWork.GetRepository<StudentStudyProcess>()
                .AsQueryable()
                .Where(x => entity.Select(e => e.Id).Contains(x.Id))
                .ToListAsync();

                foreach (var item in updateStudyProcess)
                {
                    var correspondingEntity = entity.FirstOrDefault(x => x.Id == item.Id);
                    if (correspondingEntity != null)
                    {
                        item.IsAbsent = correspondingEntity.IsAbsent;
                        item.Description = correspondingEntity.Description;
                        item.UpdatedAt = DateTime.Now;
                        item.UpdatedBy = userName;
                    }
                }

                _unitOfWork.GetRepository<StudentStudyProcess>().UpdateRange(updateStudyProcess);

            }
            else
            {
                await _unitOfWork.GetRepository<StudentStudyProcess>().AddRange(studyProcesses);

            }

            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                if (!isUpdate && learningProcess != null)
                {
                    learningProcess.Status = ELearningProcessStatus.Done;
                    learningProcess.UpdatedAt = DateTime.Now;
                    learningProcess.UpdatedBy = userName;
                    learningProcessRepository.Update(learningProcess);
                    await _unitOfWork.SaveChangesAsync();
                }
                result.Message = !isUpdate ? "Điểm danh thành công!" : "Cập nhật điểm danh thành công!";
                return result;
            }
            result.Errors.Add("Lỗi khi điểm danh !");
            return result;
        }
    }
}