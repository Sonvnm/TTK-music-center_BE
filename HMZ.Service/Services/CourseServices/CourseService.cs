
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
using System.Linq;
using System.Runtime.CompilerServices;
using HMZ.Service.Services.CloudinaryServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using CloudinaryDotNet;
using HMZ.Database.Enums;


namespace HMZ.Service.Services.CourseServices
{
    public class CourseService : ServiceBase<IUnitOfWork>, ICourseService
    {
        private readonly ICloudinaryService _cloudinaryService;
        public CourseService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider, ICloudinaryService cloudinaryService) : base(unitOfWork, serviceProvider)
        {
            _cloudinaryService = cloudinaryService;
        }

        public async Task<DataResult<int>> AddSubjectToCourse(SubjectCourseQuery entity)
        {
            var result = new DataResult<int>();

            var course = _unitOfWork.GetRepository<Course>().AsQueryable().Where(x => x.Id.Equals(Guid.Parse(entity.CourseId))).FirstOrDefault();
            var listSubject = await _unitOfWork.GetRepository<Subject>().AsQueryable().Where(x => x.IsActive == true && entity.ListSubjectId.Contains(x.Id.ToString())).ToListAsync();
            if (!listSubject.Any())
            {
                result.Errors.Add("Không có môn học nào phù hợp!");
                return result;
            }
            if (course != null)
            {
                var listSubjectExist = await _unitOfWork.GetRepository<SubjectCourse>().AsQueryable().Include("Subject").Where(x => x.CourseId.Equals(course.Id) && entity.ListSubjectId.Contains(x.SubjectId.ToString())).ToListAsync();

                if (listSubjectExist.Any())
                {
                    result.Errors.Add($"Môn học {string.Join(",", listSubjectExist.Select(x => x.Subject.Name))} đã tồn tại trong khóa học này!");
                    return result;
                }
            }




            var listSubjectCourse = new List<SubjectCourse>();
            foreach (var item in entity.ListSubjectId)
            {
                listSubjectCourse.Add(new SubjectCourse
                {
                    CourseId = course.Id,
                    SubjectId = Guid.Parse(item),
                });


            }
            await _unitOfWork.GetRepository<SubjectCourse>().AddRange(listSubjectCourse);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = $"Thêm môn học cho khóa học {course.Name} thành công!";
                return result;
            }


            return result;
        }

        public async Task<DataResult<bool>> RemoveSubjectCourse(SubjectCourseQuery query)
        {
            var result = new DataResult<bool>();
            var subjectCourseRes = _unitOfWork.GetRepository<SubjectCourse>();
            var subjectCourse = subjectCourseRes.AsQueryable().Include("Subject").Where(x => x.CourseId.ToString().Equals(query.CourseId) && query.ListSubjectId.Contains(x.SubjectId.ToString())).ToList();

            if (!subjectCourse.Any())
            {
                result.Errors.Add($"Môn học {string.Join(",", subjectCourse.Select(x => x.Code))} không tồn tại trong khóa học này!");
                return result;
            }
            subjectCourseRes.DeleteRange(subjectCourse);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = $"Xóa môn học: {string.Join(",", subjectCourse.Select(x => x.Subject.Name))} thành công !";
            }
            return result;
        }

        public async Task<DataResult<bool>> CreateAsync([FromForm] CourseQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<CourseQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            var allCourse = await _unitOfWork.GetRepository<Course>().AsQueryable().ToListAsync();

            var courseNameExist = allCourse.FirstOrDefault(x => x.Name.Equals(entity.Name.Trim()));

            if (courseNameExist != default)
            {
                result.Errors.Add("Tên khóa học này đã tồn tại!");
                return result;
            }

            var course = new Course
            {
                Name = entity.Name.Trim(),
                Description = entity.Description,
                Image = entity.Image,
                Price = entity.Price,
                StartDate = Convert.ToDateTime(entity.StartDate).Date,
                EndDate = Convert.ToDateTime(entity.EndDate).Date,
                Video = entity.Video,
                IsActive = entity.IsActive,
                PublicId = entity.PublicId,

            };

            // if (entity.IsHaveImage && entity.Image != null)
            // {
            //     if (HMZCommon.CheckImageFile(entity.Image))
            //     {
            //         var uploadResult = await _cloudinaryService.UploadImageAsync(entity.Image, "document");
            //         if (uploadResult == null)
            //         {
            //             result.Errors.Add("Upload image file fail ! Please try again");
            //             return result;
            //         }
            //         course.Image = uploadResult.Url.ToString();
            //     }
            //     else
            //     {
            //         result.Errors.Add("Image must have valid extension !");
            //         return result;
            //     }
            // }
            // Create entity


            await _unitOfWork.GetRepository<Course>().Add(course);

            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Thêm khóa học thành công";
                result.EntityId = course.Id.ToString();
                return result;
            }
            result.Errors.Add("Thêm khóa học thất bại");
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
            var courseRepository = _unitOfWork.GetRepository<Course>();
            var courses = await courseRepository.AsQueryable()
                .Where(x => id.Contains(x.Id.ToString()))
                .ToListAsync();
            // startDate > now to not delete
            if (courses.Any(x => x.StartDate <= DateTime.Now))
            {
                result.Errors.Add("Không thể xóa khóa học đã bắt đầu");
                return result;
            }

            // check class exist
            var classRes = _unitOfWork.GetRepository<Class>();
            var listClass = classRes.AsQueryable().Where(x => id.Contains(x.CourseId.ToString())).ToList();
            if (listClass.Any())
            {
                result.Errors.Add("Không thể xóa khóa học đã có lớp học");
                return result;
            }

            // get list subject by course
            var subjectCourseRes = _unitOfWork.GetRepository<SubjectCourse>();
            var listSubjectCourse = subjectCourseRes.AsQueryable().Where(x => id.Contains(x.CourseId.ToString())).ToList();
            if (listSubjectCourse.Any())
            {
                subjectCourseRes.DeleteRange(listSubjectCourse);
                await _unitOfWork.SaveChangesAsync();
            }

            if (courses == null || courses.Count == 0)
            {
                result.Errors.Add("Không tìm thấy khóa học");
                return result;
            }

            courseRepository.DeleteRange(courses, false);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                foreach (var course in courses)
                {
                    if (!course.Image.IsNullOrEmpty())
                    {
                        string[] paths = course.Image.Split("/");
                        var publicId = paths[paths.Length - 2] + "/" + paths[paths.Length - 1].Split(".")[0];
                        var checkResult = await _cloudinaryService.DeleteImageAsync(publicId);
                        if (checkResult.Result != null)
                        {
                            course.Image = null;
                        }

                    }
                }
                result.Message = "Xóa khóa học thành công";
                return result;
            }
            result.Errors.Add("Xóa khóa học thất bại");
            return result;
        }

        public async Task<DataResult<CourseView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<CourseView>();
            var course = await _unitOfWork.GetRepository<Course>().AsQueryable()
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new CourseView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    Image = x.Image,
                    Price = x.Price,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Video = x.Video,
                    Status = x.Status,
                    PublicId = x.PublicId

                }).FirstOrDefaultAsync();
            if (course == null)
            {
                result.Errors.Add("Không tìm thấy khóa học");
                return result;
            }
            result.Entity = course;
            return result;
        }

        public async Task<DataResult<CourseView>> GetByIdAsync(string id)
        {
            var result = new DataResult<CourseView>();
            var course = await _unitOfWork.GetRepository<Course>().AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true && x.Status == true)
                .Select(x => new CourseView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    Image = x.Image,
                    Price = x.Price,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Video = x.Video,
                    Status = x.Status,
                    PublicId = x.PublicId

                }).FirstOrDefaultAsync();
            if (course == null)
            {
                result.Errors.Add("Không tìm thấy khóa học");
                return result;
            }
            result.Entity = course;
            return result;
        }

        public async Task<DataResult<CourseView>> GetPageList(BaseQuery<CourseFilter> query)
        {
            var result = new DataResult<CourseView>();
            var courseQuery = _unitOfWork.GetRepository<Course>().AsQueryable()
                    .ApplyFilter(query)
                    .OrderByColumns(query.SortColumns, query.SortOrder);


            result.TotalRecords = await courseQuery.CountAsync();
            result.Items = await courseQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new CourseView(x)
                    {
                        Name = x.Name,
                        Description = x.Description,
                        Image = x.Image,
                        Price = x.Price,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Video = x.Video,
                        Status = x.Status,
                        PublicId = x.PublicId
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<CourseView>> GetSubjectByCoursePageList(BaseQuery<SubjectQuery> query)
        {
            var result = new DataResult<CourseView>();
            var course = await _unitOfWork.GetRepository<Course>().AsQueryable()
            .FirstOrDefaultAsync(x => x.Id == query.Entity.CourseId);
            if (course == null)
            {
                result.Errors.Add("Không tìm thấy khóa học");
                return result;
            }
            var subjectQuery = _unitOfWork.GetRepository<Subject>().AsQueryable()
                .Include(x => x.SubjectCourses)
                .Where(x => x.SubjectCourses.Any(x => x.CourseId.Equals(query.Entity.CourseId)));
            result.TotalRecords = await subjectQuery.CountAsync();

            var subjects = await subjectQuery.Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .OrderByColumns(query.SortColumns, query.SortOrder)
                    .Select(x => new SubjectView(x)
                    {
                        Name = x.Name,
                        Description = x.Description

                    }).ToListAsync();
            var entity = new CourseView(course)
            {
                Name = course.Name,
                Description = course.Description,
                Image = course.Image,
                Price = course.Price,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Video = course.Video,
                Status = course.Status,
                Subjects = subjects,
                PublicId = course.PublicId
            };
            result.Entity = entity;
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(CourseQuery entity, string id)
        {
            var result = new DataResult<int>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<CourseQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity,isUpdate:true);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Update entity
            var allCourse =  _unitOfWork.GetRepository<Course>().AsQueryable();
            
            var course = await allCourse.FirstOrDefaultAsync(x=>x.Id.ToString().Equals(id));
            if (entity.StartDate < course.StartDate)
            {
                result.Errors.Add("Ngày bắt đầu mới không được nhỏ hơn ngày bắt đầu cũ");
                return result;
            }
            if (course == null)
            {
                result.Errors.Add("Không tìm thấy khóa học");
                return result;
            }

            var courseNameExist = await allCourse.FirstOrDefaultAsync(x => x.Name.Equals(entity.Name) && x.Id != course.Id);

            if (courseNameExist != default)
            {
                result.Errors.Add("Tên khóa học này đã tồn tại!");
                return result;
            }

            if (entity.Image != null && entity.Image != course.Image && entity.Image != null)
            {
                if (course.PublicId != null)
                {
                    var checkResult = await _cloudinaryService.DeleteImageAsync(course.PublicId);
                    if (checkResult.Result != null)
                    {
                        course.Image = null;
                    }
                }
            }
            course.Name = entity.Name;
            course.Description = entity.Description;
            course.Price = entity.Price;
            course.StartDate = entity.StartDate.Value.Date;
            course.EndDate = entity.EndDate.Value.Date;
            course.Video = entity.Video;
            course.Status = entity.Status;
            course.Image = entity.Image.IsNullOrEmpty() ? course.Image : entity.Image;
            course.PublicId = entity.PublicId.IsNullOrEmpty() ? course.PublicId : entity.PublicId;

            course.UpdatedBy = entity.CreatedBy;
            course.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<Course>().Update(course);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Cập nhật khóa học thành công";
                return result;
            }
            result.Errors.Add("Cập nhật khóa học thất bại");
            return result;
        }

        public async Task<DataResult<SubjectCourseView>> GetAllForStudent()
        {
            var result = new DataResult<SubjectCourseView>();
            var subjectCourses = await _unitOfWork.GetRepository<SubjectCourse>().AsQueryable().Include(x => x.Course).Include(x => x.Subject).GroupBy(x => x.Course).ToListAsync();
            var subjectCoursesAvailable = subjectCourses.Where(x => x.Key.IsActive == true && x.Key.Status == true && DateTime.Compare(DateTime.Now, (DateTime)x.Key.StartDate) <= 0).Select(x => new SubjectCourseView()
            {
                CourseCode = x.Select(x => x.Course.Code).FirstOrDefault(),
                CourseId = x.Select(x => x.CourseId).FirstOrDefault(),
                Course = x.Key,
                Subjects = x.Select(x => x.Subject).ToList()
            }).ToList();

            if (subjectCoursesAvailable.Any())
            {
                result.TotalRecords = subjectCoursesAvailable.Count();
                result.Items = subjectCoursesAvailable;
            }

            return result;

        }

        public async Task<DataResult<SubjectCourseView>> GetSubjectCourseById(string id, string userName)
        {
            var result = new DataResult<SubjectCourseView>();
            var orders = new List<Order>();
            if (userName != null)
            {
                var user = await _unitOfWork.GetRepository<User>().AsQueryable().Where(x => x.UserName.Equals(userName)).FirstOrDefaultAsync();
                orders = await _unitOfWork.GetRepository<Order>().AsQueryable().Include(x => x.OrderDetails).Where(x => x.UserId.Equals(user.Id) && x.Status.Equals(EOrderStatus.Done)).ToListAsync();
            }
            var subjectCourse = await _unitOfWork.GetRepository<SubjectCourse>().AsQueryable().Where(x => x.CourseId.ToString().Equals(id)).Include(x => x.Course).Include(x => x.Subject).GroupBy(x => x.Course).ToListAsync();
            var courseClasses = await _unitOfWork.GetRepository<Class>().AsQueryable().Where(x => x.CourseId.ToString().Equals(id)).Include(x => x.StudentClasses).ThenInclude(x => x.User).ToListAsync();


            var subjectCourseAvailable = subjectCourse.Select(x => new SubjectCourseView()
            {
                CourseCode = x.Select(x => x.Course.Code).FirstOrDefault(),
                CourseId = x.Select(x => x.CourseId).FirstOrDefault(),
                Course = x.Key,
                Subjects = x.Select(x => x.Subject).ToList(),
                IsBought = orders.Any(x => x.OrderDetails.Select(y => y.CourseId.Equals(Guid.Parse(id))).FirstOrDefault()),
                Classes = courseClasses,
                RatingAvailable = DateTime.Compare(DateTime.Now, (DateTime)x.Select(x => x.Course.EndDate).FirstOrDefault()) >= 0

            }).FirstOrDefault();

            if (subjectCourseAvailable != null)
            {
                result.Entity = subjectCourseAvailable;
            }

            return result;


        }
    }
}