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
using HMZ.Service.Services.CloudinaryServices;

using HMZ.Service.Services.DocumentServices;


namespace HMZ.Service.Services.ClassServices
{
    public class ClassService : ServiceBase<IUnitOfWork>, IClassService
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IDocumentService _documentService;
        public ClassService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider, ICloudinaryService cloudinaryService, IDocumentService documentService) : base(unitOfWork, serviceProvider)
        {
            _cloudinaryService = cloudinaryService;
            _documentService = documentService;
        }

        public async Task<DataResult<int>> AddStudentToClass(StudentClassQuery query, bool isTeacher = false)
        {
            var result = new DataResult<int>();
            var users = await _unitOfWork.GetRepository<User>().AsQueryable()
                .Where(x => query.UserCode.Contains(x.Code) && x.IsActive == true)
                .ToListAsync();
            if (users == null || users.Count == 0)
            {
                result.Errors.Add("Không tìm thấy người dùng");
                return result;
            }

            var classs = await _unitOfWork.GetRepository<Class>().AsQueryable()
                .Where(x => x.Code == query.ClassCode && x.IsActive == true).Include(x => x.Course)
                .FirstOrDefaultAsync();
            if (classs == null)
            {
                result.Errors.Add("Không tìm thấy lớp học");
                return result;
            }
            var studentClassQuery = _unitOfWork.GetRepository<StudentClass>().AsQueryable();

            long userInClass = await studentClassQuery
                .Where(x => x.ClassId == classs.Id && x.Role == EPersonRoles.Student)
                .LongCountAsync();
            if (userInClass + users.Count > 4 && !isTeacher)
            {
                result.Errors.Add("Lớp học đã đủ số lượng học viên");
                return result;
            }

            // chech one teacher one class
            var userInClassTeacher = await studentClassQuery
                .Where(x => x.ClassId == classs.Id && x.Role == EPersonRoles.Teacher)
                .LongCountAsync();
            if (userInClassTeacher > 0 && isTeacher)
            {
                result.Errors.Add("Lớp học đã có giáo viên");
                return result;
            }


            var studentClass = await studentClassQuery
                .Include(x => x.User)
                .Where(x => x.ClassId == classs.Id)
                .ToListAsync();

            if (!isTeacher && !studentClass.Any(x => x.Role == EPersonRoles.Teacher))
            {
                result.Errors.Add("Vui lòng thêm giáo viên vào lớp học!");
                return result;
            }

            // check user in other class
            var userOtherClass = await _unitOfWork.GetRepository<StudentClass>().AsQueryable()
                .Include(x => x.Class).ThenInclude(x => x.Course).ThenInclude(x => x.Schedules)
                .Include(x => x.User)
                .Where(x => query.UserCode.Contains(x.User.Code) && x.ClassId != classs.Id
                    && x.IsActive == true && x.Role == EPersonRoles.Student
                )
                .ToListAsync();

            if (userOtherClass.Any())
            {
                var userOtherClassName = userOtherClass.Select(x => x.User.UserName).Distinct().ToList();
                foreach (var item in userOtherClass)
                {
                    if (item.Class.Course.Id.Equals(classs.Course.Id))
                    {
                        if (userOtherClass != null && userOtherClass.Count > 0)
                        {
                            result.Errors.Add($"Người dùng {string.Join(", ", userOtherClassName)} đã tồn tại trong lớp học khác");
                            return result;
                        }

                    }
                    else if (item.Class.Course.Equals(classs.Course.Id) && item.Class.Course.EndDate.Value.Date <= DateTime.Now.Date)
                    {
                        result.Errors.Add($"Người dùng {string.Join(", ", userOtherClassName)} đã tồn tại trong lớp học có khóa học chưa được kết thúc");
                        return result;
                    }
                }

            }





            var studentClassExits = studentClass.Where(x => query.UserCode.Contains(x.User.Code)).ToList();
            if (studentClass == null || studentClassExits.Count > 0)
            {

                string userName = string.Join(", ", studentClassExits.Select(x => x.User.UserName));
                result.Errors.Add($"Người dùng {userName} đã tồn tại trong lớp học");
                return result;
            }
            var userClass = new List<StudentClass>();
            foreach (var user in users)
            {
                userClass.Add(new StudentClass
                {
                    ClassId = classs.Id,
                    UserId = user.Id,
                    Role = isTeacher ? EPersonRoles.Teacher : EPersonRoles.Student,
                    CreatedBy = query.CreatedBy,
                });
            }

            await _unitOfWork.GetRepository<StudentClass>().AddRange(userClass);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = $"Thêm {userClass.Count} người dùng vào lớp học {classs.Name} thành công";
                return result;
            }
            result.Errors.Add("Thêm người dùng vào lớp học thất bại");
            return result;
        }

        public async Task<DataResult<bool>> CreateAsync(ClassQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<ClassQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }

            var classRepository = _unitOfWork.GetRepository<Class>();

            var classes = await classRepository.AsQueryable().ToListAsync();
            if (classes.Any(x => x.CourseId.Equals(entity.CourseId) && x.Name.Equals(entity.Name)))
            {
                result.Errors.Add($"Tên lớp {entity.Name} bị trùng trong khóa học này! Vui lòng đặt tên khác");
                return result;
            }
            // Create entity

            var data = new Class
            {
                Name = entity.Name,
                Description = entity.Description,
                CourseId = entity.CourseId,
                CreatedBy = entity.CreatedBy,
            };
            await _unitOfWork.GetRepository<Class>().Add(data);

            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Thêm lớp học thành công";
                return result;
            }
            result.Errors.Add("Thêm lớp học thất bại");
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
            var classRepository = _unitOfWork.GetRepository<Class>();
            var scheduleRepository = _unitOfWork.GetRepository<Schedule>();
            var classs = await classRepository.AsQueryable()
                .Where(x => id.Contains(x.Id.ToString()) && x.IsActive == true)
                .ToListAsync();

            if (classs == null || classs.Count == 0)
            {
                result.Errors.Add("Không tìm thấy lớp học");
                return result;
            }

            // check student in class
            var studentClass = await _unitOfWork.GetRepository<StudentClass>().AsQueryable()
                .Where(x => id.Contains(x.ClassId.ToString())).Include(x => x.Class)
                .ToListAsync();
            if (studentClass != null && studentClass.Count > 0)
            {
                result.Errors.Add($"Không thể xóa lớp {studentClass.Select(x => x.Class.Name).FirstOrDefault()} khi có người dùng đang tồn tại");
                return result;
            }
            // check document in class


            var scheduleClass = await scheduleRepository.AsQueryable()
                .Where(x => id.Contains(x.ClassId.ToString())).Include(x => x.Class)
                .ToListAsync();
            if (scheduleClass.Any())
            {
                result.Errors.Add($"Lớp {string.Join(",", scheduleClass.Select(x => x.Class.Name))} đã có lịch dạy không thể xóa");
                return result;
            }
            var document = await _unitOfWork.GetRepository<Document>().AsQueryable()
              .Where(x => id.Contains(x.ClassId.ToString()))
              .ToListAsync();
            if (document != null && document.Count > 0)
            {
                _unitOfWork.GetRepository<Document>().DeleteRange(document, false);
            }

            classRepository.DeleteRange(classs);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa lớp học thành công";
                // remove document in by cloudinary
                foreach (var item in document)
                {
                    await _cloudinaryService.DeleteFileAsync(item.PublicId);
                }
                return result;
            }
            result.Errors.Add("Xóa lớp học thất bại");
            return result;
        }



        public async Task<DataResult<ClassView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<ClassView>();
            var classs = await _unitOfWork.GetRepository<Class>().AsQueryable()
                .Include(x => x.Course)
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new ClassView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    Course = new CourseView(x.Course)
                    {
                        Name = x.Course.Name,
                        Description = x.Course.Description,
                        EndDate = x.Course.EndDate,
                        StartDate = x.Course.StartDate,
                        Image = x.Course.Image,
                        Video = x.Course.Video,
                        Price = x.Course.Price,
                    }

                }).FirstOrDefaultAsync();
            if (classs == null)
            {
                result.Errors.Add("Không tìm thấy lớp học");
                return result;
            }
            result.Entity = classs;
            return result;
        }

        public async Task<DataResult<ClassView>> GetByIdAsync(string id)
        {
            var result = new DataResult<ClassView>();
            var classs = await _unitOfWork.GetRepository<Class>().AsQueryable()
                .Include(x => x.Course)
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .Select(x => new ClassView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    Course = new CourseView(x.Course)
                    {
                        Name = x.Course.Name,
                        Description = x.Course.Description,
                        EndDate = x.Course.EndDate,
                        StartDate = x.Course.StartDate,
                        Image = x.Course.Image,
                        Video = x.Course.Video,
                        Price = x.Course.Price,
                    }

                }).FirstOrDefaultAsync();
            if (classs == null)
            {
                result.Errors.Add("Không tìm thấy lớp học");
                return result;
            }
            result.Entity = classs;
            return result;
        }

        public async Task<DataResult<ClassView>> GetClassesByUserId(string userId)
        {
            var result = new DataResult<ClassView>();
            var classes = await _unitOfWork.GetRepository<Class>().AsQueryable()
                .Include(x => x.Course).Include(x => x.StudentClasses).ThenInclude(x => x.User).ToListAsync();
            var classView = classes.Where(x => x.StudentClasses.Any(y => y.UserId.ToString() == userId))

                .Select(x => new ClassView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    Teachers = x.StudentClasses.Where(x => x.Role == EPersonRoles.Teacher).ToList(),
                    Course = new CourseView(x.Course)
                    {
                        Name = x.Course.Name,
                        Description = x.Course.Description,
                        EndDate = x.Course.EndDate,
                        StartDate = x.Course.StartDate,
                        Image = x.Course.Image,
                        Video = x.Course.Video,
                        Price = x.Course.Price,
                    }

                }).ToList();


            if (classView == null)
            {
                result.Errors.Add("Không tìm thấy lớp học");
                return result;
            }
            result.Items = classView;
            return result;

        }

        public async Task<DataResult<ClassView>> GetPageList(BaseQuery<ClassFilter> query)
        {

            var result = new DataResult<ClassView>();
            // filter column not in table
            string queryFilter = query?.Entity?.CourseName;
            if (query.Entity != null && !string.IsNullOrEmpty(queryFilter))
                query.Entity.CourseName = null;

            // get user login
            var user = await GetUserLoginAsync();
            if (user == null)
            {
                result.Errors.Add("Không tìm thấy người dùng");
                return result;
            }
            // get role user
            var role = await _unitOfWork.GetRepository<UserRole>().AsQueryable()
                .Include(x => x.Role)
                .Where(x => x.UserId == user.Id)
                .Select(x => x.Role.Name)
                .ToListAsync();

            var classQuery = _unitOfWork.GetRepository<Class>().AsQueryable();
            if (role.Contains(EUserRoles.Admin.ToString()))
            {
                classQuery = classQuery.Include(x => x.Course)
                .Where(x => queryFilter == null || x.Course.Name.Contains(queryFilter))
                .ApplyFilter(query)
                .OrderByColumns(query.SortColumns, query.SortOrder);
            }
            else
            {

                var studentClass = _unitOfWork.GetRepository<StudentClass>().AsQueryable()
                                .Include(x => x.Class)
                                .Include(x => x.User)
                                .Where(x => x.User.Id == user.Id && x.Role == EPersonRoles.Teacher)
                                .Select(x => x.Class.Code);

                var dataQR = classQuery
                                 .Where(x => (queryFilter == null || x.Course.Name.Contains(queryFilter))
                                     && studentClass.Contains(x.Code)
                                 )
                                 .ApplyFilter(query)
                                 .OrderByColumns(query.SortColumns, query.SortOrder);
                result.TotalRecords = await dataQR.CountAsync();
                result.Items = await dataQR
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new ClassView(x)
                    {
                        Name = x.Name,
                        Description = x.Description,
                        CourseName = x.Course.Name,
                        CourseId = x.Course.Id,
                        Course = new CourseView(x.Course)
                        {
                            Name = x.Course.Name,
                            Description = x.Course.Description,
                            EndDate = x.Course.EndDate,
                            StartDate = x.Course.StartDate,
                            Image = x.Course.Image,
                            Video = x.Course.Video,
                            Price = x.Course.Price,
                        }
                    }).ToListAsync();
                return result;
            }

            result.TotalRecords = await classQuery.CountAsync();
            result.Items = await classQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new ClassView(x)
                    {
                        Name = x.Name,
                        Description = x.Description,
                        CourseName = x.Course.Name,
                        CourseId = x.Course.Id,
                        Course = new CourseView(x.Course)
                        {
                            Name = x.Course.Name,
                            Description = x.Course.Description,
                            EndDate = x.Course.EndDate,
                            StartDate = x.Course.StartDate,
                            Image = x.Course.Image,
                            Video = x.Course.Video,
                            Price = x.Course.Price,
                        }
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<UserView>> GetStudentByClassPageList(BaseQuery<StudentQuery> query)
        {
            var result = new DataResult<UserView>();

            // Extracting query.Entity values
            var firstNameQuery = query.Entity.FirstName;
            var lastNameQuery = query.Entity.LastName;
            var roleNameQuery = query.Entity.RoleName;
            var emailQuery = query.Entity.Email;

            // Reset query.Entity values
            query.Entity.FirstName = null;
            query.Entity.LastName = null;
            query.Entity.RoleName = null;
            query.Entity.Email = null;

            // Get the class
            var classs = await _unitOfWork.GetRepository<Class>()
                .AsQueryable()
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.Code == query.Entity.ClassCode && x.IsActive == true);

            if (classs == null)
            {
                result.Errors.Add("Không tìm thấy lớp học");
                return result;
            }

            // Get students for the class
            var studentsQuery = _unitOfWork.GetRepository<StudentClass>().AsQueryable()
                .Include(x => x.User).ThenInclude(x => x.UserRoles).ThenInclude(x => x.Role)
                .Where(x => x.ClassId == classs.Id && x.IsActive == true);

            // Apply filters
            if (!string.IsNullOrEmpty(firstNameQuery))
                studentsQuery = studentsQuery.Where(x => x.User.FirstName.Contains(firstNameQuery));
            if (!string.IsNullOrEmpty(lastNameQuery))
                studentsQuery = studentsQuery.Where(x => x.User.LastName.Contains(lastNameQuery));
            if (!string.IsNullOrEmpty(roleNameQuery))
                studentsQuery = studentsQuery.Where(x => x.User.UserRoles.Any(role => role.Role.Name.Contains(roleNameQuery)));
            if (!string.IsNullOrEmpty(emailQuery))
                studentsQuery = studentsQuery.Where(x => x.User.Email.Contains(emailQuery));


            // Count total records
            result.TotalRecords = await studentsQuery.CountAsync();

            // Apply pagination and projection
            var students = await studentsQuery
                .OrderBy(x => x.Id) // Change to appropriate ordering
                .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                .Take(query.PageSize.Value)
                .Select(x => new UserView
                {
                    Id = x.Id,
                    Code = x.User.Code,
                    Username = x.User.UserName,
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    DateOfBirth = x.User.DateOfBirth,
                    Email = x.User.Email,
                    Image = x.User.Image,
                    RoleName = x.Role.ToString()
                })
                .ToListAsync();

            result.Items = students;
            return result;

        }

        public async Task<DataResult<UserView>> GetStudentByPageList(BaseQuery<StudentQuery> query, bool isTeacher = false)
        {
            var result = new DataResult<UserView>();
            var userIdExits = _unitOfWork.GetRepository<StudentClass>().AsQueryable()
                .Include(x => x.Class)
                .Include(x => x.User)
                .Where(x => x.Class.Code == query.Entity.ClassCode)
                .Select(x => x.UserId).ToList();

            var courseId = _unitOfWork.GetRepository<Class>().AsQueryable().Where(x => x.Code == query.Entity.ClassCode).Select(x => x.CourseId).FirstOrDefault();
            var studentIdsFree = _unitOfWork.GetRepository<StudentClass>().AsQueryable()
            .Include(x => x.Class)
            .Where(x => x.Class.Code == query.Entity.ClassCode)
            .Select(x => x.UserId).ToList();

            var userIds = _unitOfWork.GetRepository<OrderDetail>().AsQueryable().Include(x => x.Order).Where(x => x.CourseId.Equals(courseId) && x.Order.Status == EOrderStatus.Done && !studentIdsFree.Contains(x.Order.UserId)).Select(x => x.Order.UserId).ToList();

            // filter column not in table
            string queryFilter = query?.Entity?.ClassCode;
            if (query.Entity != null && !string.IsNullOrEmpty(queryFilter))
                query.Entity.ClassCode = null;
            // filter column not in table    
            var userQuery = isTeacher ? _unitOfWork.GetRepository<User>().AsQueryable()
                .Include(x => x.UserRoles).ThenInclude(x => x.Role)
                .Where(x => !userIdExits.Contains(x.Id)
                    && x.IsActive == true
                    && x.UserRoles.Any(x => x.Role.Name == EUserRoles.Teacher.ToString())
                )
                .ApplyFilter(query) :
                _unitOfWork.GetRepository<User>().AsQueryable()
                .Include(x => x.UserRoles).ThenInclude(x => x.Role)
                .Where(x => userIds.Contains(x.Id)
                    && x.IsActive == true
                    && !x.UserRoles.Any(x => x.Role.Name == EUserRoles.Admin.ToString() || x.Role.Name == EUserRoles.Teacher.ToString())
                );

            result.TotalRecords = await userQuery.CountAsync();
            result.Items = await userQuery
                .OrderByColumns(query.SortColumns, query.SortOrder)
                .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                .Take(query.PageSize.Value)
                .Select(x => new UserView()
                {
                    Id = x.Id,
                    Code = x.Code,
                    Username = x.UserName,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    DateOfBirth = x.DateOfBirth,
                    Email = x.Email,
                    Image = x.Image,
                }).ToListAsync();
            return result;
        }

        public async Task<DataResult<int>> RemoveStudentFromClass(StudentClassQuery query)
        {
            var result = new DataResult<int>();
            var classs = await _unitOfWork.GetRepository<Class>().AsQueryable()
                .Where(x => x.Code == query.ClassCode && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (classs == null)
            {
                result.Errors.Add("Không tìm thấy lớp học");
                return result;
            }

            var studentClass = await _unitOfWork.GetRepository<StudentClass>().AsQueryable()
                .Where(x => x.ClassId == classs.Id && query.UserCode.Contains(x.User.Code))
                .ToListAsync();
            if (studentClass == null || studentClass.Count == 0)
            {
                result.Errors.Add("Không tìm thấy người dùng trong lớp học");
                return result;
            }
            _unitOfWork.GetRepository<StudentClass>().DeleteRange(studentClass, false); // false: delete in db
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa người dùng khỏi lớp học thành công";
                return result;
            }
            result.Errors.Add("Xóa người dùng khỏi lớp học thất bại");
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(ClassQuery entity, string id)
        {
            var result = new DataResult<int>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<ClassQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity, isUpdate: true);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Update entity
            var classRepository = _unitOfWork.GetRepository<Class>();
            var classs = await classRepository.AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (classs == null)
            {
                result.Errors.Add("Không tìm thấy lớp học");
                return result;
            }
            var classes = await classRepository.AsQueryable().ToListAsync();
            if (classes.Any(x => x.CourseId.Equals(entity.CourseId) && x.Name.Equals(entity.Name)) && !entity.Name.Equals(classs.Name))
            {
                result.Errors.Add($"Tên lớp {entity.Name} bị trùng trong khóa học này! Vui lòng đặt tên khác");
                return result;
            }
            classs.Name = entity.Name;
            classs.Description = entity.Description;
            classs.CourseId = entity.CourseId;

            classs.UpdatedBy = entity.UpdatedBy;
            classs.UpdatedAt = DateTime.Now;
            classRepository.Update(classs);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Cập nhật lớp học thành công";
                return result;
            }
            result.Errors.Add("Cập nhật lớp học thất bại");
            return result;
        }
        public async Task<DataResult<int>> DeleteDocumentsFromClass(string userName, string[] documentIds)
        {
            var result = new DataResult<int>();

            var user = await _unitOfWork.GetRepository<User>().AsQueryable().Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.UserName.Equals(userName));
            if (user == null)
            {
                return result;
            }
            var isAdmin = user.UserRoles.Any(x => x.Role.Name == "Admin");

            var documents = await _unitOfWork.GetRepository<Document>().AsQueryable().ToListAsync();

            if (!isAdmin)
            {
                foreach (var documentId in documentIds)
                {
                    var document = documents.Where(x => x.Id.ToString().Equals(documentId)).FirstOrDefault();
                    var userUpload = await _unitOfWork.GetRepository<User>().AsQueryable().Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.Id.Equals(document.UserId));

                    if (document != null && userUpload != null && userUpload.UserRoles.Select(x => x.Role.Name == "Admin").FirstOrDefault())
                    {
                        result.Errors.Add("Không thể xóa tài liệu của người khác !");
                        return result;
                    }
                }
            }

            result = await _documentService.DeleteAsync(documentIds);
            if (result.Errors.Any())
            {
                return result;
            }
            return result;
        }

        public async Task<DataResult<ClassView>> GetClassesByCourse(string courseId)
        {
            var result = new DataResult<ClassView>();
            var classes = await _unitOfWork.GetRepository<Class>().AsQueryable()
                .Include(x => x.Course).Include(x => x.StudentClasses).ThenInclude(x => x.User).ToListAsync();
            if (classes != null)
            {
                var classView = classes.Where(x => x.CourseId.Equals(Guid.Parse(courseId))).ToList();
                if (!classView.Any())
                {
                    result.Errors.Add("Chưa tồn tại lớp học trong khóa học này !");
                    return result;

                }

            }
            var items = classes.Where(x => x.CourseId.Equals(Guid.Parse(courseId)))

                .Select(x => new ClassView(x)
                {
                    Name = x.Name,
                    Id = x.Id
                }).ToList();



            result.Items = items;
            return result;
        }

        public async Task<DataResult<ClassView>> GetStudentsClassByClassId(Guid classId)
        {
            var result = new DataResult<ClassView>();

            // Retrieve classes with eager loading of related entities
            var classes = await _unitOfWork.GetRepository<Class>()
                .AsQueryable()
                .Include(x => x.Course)
                .Include(x => x.StudentClasses)
                    .ThenInclude(x => x.User)
                .Where(x => x.Id == classId) // Filter by classId
                .FirstOrDefaultAsync();

            if (classes == default)
            {
                result.Errors.Add("Không tìm thấy lớp học với ID đã cung cấp.");
                return result;
            }

            // Assuming each class can have multiple students
            var classViewWithStudents = new ClassView(classes)
            {
                StudentClasses = classes.StudentClasses.Where(x => x.Role.Equals(EPersonRoles.Student)).ToList()
            };

            result.Items = new List<ClassView> { classViewWithStudents };
            return result;
        }
    }


}