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
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HMZ.Service.Services.SubjectServices
{
    public class SubjectService : ServiceBase<IUnitOfWork>, ISubjectService
    {
        public SubjectService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(unitOfWork, serviceProvider)
        {
        }

        public async Task<DataResult<bool>> CreateAsync(SubjectQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<SubjectQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Create entity
            var subjects = await _unitOfWork.GetRepository<Subject>().AsQueryable().ToListAsync();
            if (subjects.Any(x => x.Name.Equals(entity.Name)))
            {
                result.Errors.Add("Tên môn học đã trùng lặp , vui lòng chọn tên khác ");
                return result;
            }
            var subject = new Subject
            {
                Name = entity.Name,
                Description = entity.Description,
                CreatedBy = entity.CreatedBy,
            };

            await _unitOfWork.GetRepository<Subject>().Add(subject);
            // add SubjectCourse
            if (entity.CourseId.HasValue)
            {
                var subjectCourse = new SubjectCourse
                {
                    SubjectId = subject.Id,
                    CourseId = entity.CourseId
                };
                await _unitOfWork.GetRepository<SubjectCourse>().Add(subjectCourse);
            }

            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Thêm môn học thành công";
                return result;
            }
            result.Errors.Add("Thêm môn học không thành công");
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
            var subjectRepository = _unitOfWork.GetRepository<Subject>();
            var subjects = await subjectRepository.AsQueryable().Include(x => x.SubjectCourses).ThenInclude(x => x.Course)
                .Where(x => id.Contains(x.Id.ToString()) && x.IsActive == true)
                .ToListAsync();
            if (subjects == null || subjects.Count == 0)
            {
                result.Errors.Add("Không tìm thấy môn học");
                return result;
            }

            foreach (var subject in subjects)
            {
                if (subject.SubjectCourses.Any())
                {
                    result.Errors.Add($"Môn học {subject.Name} đang tồn tại trong khóa {string.Join(",", subject.SubjectCourses.Select(x => x.Course.Name))} không thể xóa");
                    return result;
                }
            }
            var documentsSubject = await _unitOfWork.GetRepository<Document>().AsQueryable().Where(x => id.Contains(x.SubjectId.ToString())).Include(x => x.Subject).ToListAsync();

            if (documentsSubject.Any())
            {
                result.Errors.Add($"Môn học {string.Join(",", documentsSubject.Select(x => x.Subject.Name))} đang tồn tại tài liệu nên không thể xóa! Vui lòng xóa tài liệu trước khi xóa môn học");
                return result;
            }

            subjectRepository.DeleteRange(subjects);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa môn học thành công";
                return result;
            }
            result.Errors.Add("Xóa môn học không thành công");
            return result;
        }



        public async Task<DataResult<SubjectView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<SubjectView>();
            var subject = await _unitOfWork.GetRepository<Subject>().AsQueryable()
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new SubjectView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                }).FirstOrDefaultAsync();
            if (subject == null)
            {
                result.Errors.Add("Không tìm thấy môn học");
                return result;
            }
            result.Entity = subject;
            return result;
        }

        public async Task<DataResult<SubjectView>> GetByIdAsync(string id)
        {
            var result = new DataResult<SubjectView>();
            var subject = await _unitOfWork.GetRepository<Subject>().AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .Select(x => new SubjectView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                }).FirstOrDefaultAsync();
            if (subject == null)
            {
                result.Errors.Add("Không tìm thấy môn học");
                return result;
            }
            result.Entity = subject;
            return result;
        }

        public async Task<DataResult<SubjectView>> GetPageList(BaseQuery<SubjectFilter> query)
        {
            var result = new DataResult<SubjectView>();
            var subjectQuery = _unitOfWork.GetRepository<Subject>().AsQueryable()
                     .Where(x => x.IsActive == true)
                     .ApplyFilter(query)
                     .OrderByColumns(query.SortColumns, query.SortOrder);

            result.TotalRecords = await subjectQuery.CountAsync();
            result.Items = await subjectQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new SubjectView(x)
                    {
                        Name = x.Name,
                        Description = x.Description,
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<SubjectView>> GetSubjectsForCourse(BaseQuery<SubjectFilter> query, string courseId)
        {
            var result = new DataResult<SubjectView>();
            var subjectIdsCourse = await _unitOfWork.GetRepository<SubjectCourse>().AsQueryable().Include(x => x.Subject).Where(x => x.CourseId.ToString().Equals(courseId)).Select(x => x.SubjectId).ToListAsync();
            var subjectQuery = _unitOfWork.GetRepository<Subject>().AsQueryable()
                    .Where(x => x.IsActive == true && !subjectIdsCourse.Contains(x.Id))
           .ApplyFilter(query)
           .OrderByColumns(query.SortColumns, query.SortOrder);

            result.TotalRecords = await subjectQuery.CountAsync();
            result.Items = await subjectQuery
            .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new SubjectView(x)
                    {
                        Name = x.Name,
                        Description = x.Description,
                    }).ToListAsync();

            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(SubjectQuery entity, string id)
        {
            var result = new DataResult<int>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<SubjectQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Update entity
            var subjectRepository = _unitOfWork.GetRepository<Subject>();
            var subject = await subjectRepository.AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (subject == null)
            {
                result.Errors.Add("Không tìm thấy môn học");
                return result;
            }
            var subjects = await _unitOfWork.GetRepository<Subject>().AsQueryable().ToListAsync();
            if (!subject.Name.Equals(entity.Name) && subjects.Any(x => x.Name.Equals(entity.Name)))
            {
                result.Errors.Add("Tên môn học đã trùng lặp , vui lòng chọn tên khác ");
                return result;
            }
            subject.Name = entity.Name;
            subject.Description = entity.Description;
            subject.UpdatedBy = entity.UpdatedBy;
            subject.UpdatedAt = DateTime.Now;
            subjectRepository.Update(subject);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                return result;
            }
            result.Errors.Add("Cập nhật môn học không thành công");
            return result;
        }
    }
}