
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
using System.ComponentModel.Design;
using HMZ.Service.Services.CloudinaryServices;


namespace HMZ.Service.Services.DocumentServices
{
    public class DocumentService : ServiceBase<IUnitOfWork>, IDocumentService
    {
        private readonly ICloudinaryService _cloudinaryService;
        public DocumentService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider, ICloudinaryService cloudinaryService) : base(unitOfWork, serviceProvider)
        {
            _cloudinaryService = cloudinaryService;
        }

        public async Task<DataResult<bool>> CreateAsync(DocumentQuery entity)
        {
            var result = new DataResult<bool>();
            entity.Name = entity.File.FileName;
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<DocumentQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }

            // check Class
            var classEntity = new Class();
            var subject = new Subject();
            if (entity.ClassCode != null)
            {
                var classRepository = _unitOfWork.GetRepository<Class>();
                classEntity = await classRepository.AsQueryable()
                   .Where(x => x.Code == entity.ClassCode && x.IsActive == true)
                   .FirstOrDefaultAsync();
                if (classEntity == null)
                {
                    result.Errors.Add("Không tìm thấy lớp học");
                    return result;
                }
            }
            else if (entity.SubjectCode != null)
            {
                var subjectRepository = _unitOfWork.GetRepository<Subject>();
                subject = await subjectRepository.AsQueryable()
                   .Where(x => x.Code == entity.SubjectCode && x.IsActive == true)
                   .FirstOrDefaultAsync();
                if (subject == null)
                {
                    result.Errors.Add("Không tìm thấy môn học");
                    return result;
                }
            }

            if (HMZCommon.CheckDocumentFile(entity.File))
            {
                var uploadResult = await _cloudinaryService.UploadFile(entity.File, "document");
                if (uploadResult == null)
                {
                    result.Errors.Add("Upload file thất bại");
                    return result;
                }
                entity.FilePath = uploadResult.Url.ToString();
                entity.Thumbnail = uploadResult.Uri.ToString();
                entity.FileSize = (int)uploadResult.Length;
                entity.FileExtension = Path.GetExtension(uploadResult.PublicId);
                entity.PublicId = uploadResult.PublicId;
            }
            else
            {
                result.Errors.Add("File không hợp lệ");
                return result;
            }

            // userLogin
            var userLogin = await GetUserLoginAsync();
            if (userLogin == null)
            {
                result.Errors.Add("Vui lòng đăng nhập");
                return result;
            }
            // Create entity

            var data = new Document
            {
                Name = entity.Name.Length <= 40 ? entity.Name : entity.Name.Substring(0, 35),
                Description = entity.Description,
                FileSize = entity.FileSize,
                FileExtension = entity.FileExtension,
                FilePath = entity.FilePath,
                Thumbnail = entity.Thumbnail,
                IsPublic = entity.IsPublic,
                UserId = userLogin.Id,
                ClassId = entity.ClassCode != null ? classEntity.Id : null,
                CreatedBy = userLogin.UserName,
                PublicId = entity.PublicId,
                SubjectId = entity.SubjectCode != null ? subject.Id : null,
            };
            await _unitOfWork.GetRepository<Document>().Add(data);

            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Thêm tài liệu thành công";
                return result;
            }
            result.Errors.Add("Thêm tài liệu thất bại");
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
            var documentRepository = _unitOfWork.GetRepository<Document>();
            var documents = await documentRepository.AsQueryable()
                .Where(x => id.Contains(x.Id.ToString()) && x.IsActive == true)
                .ToListAsync();

            if (documents == null || documents.Count == 0)
            {
                result.Errors.Add("Không tìm thấy tài liệu");
                return result;
            }
            documentRepository.DeleteRange(documents);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                // remove file in cloudinary
                foreach (var document in documents)
                {
                    await _cloudinaryService.DeleteImageAsync(document.PublicId);
                }
                result.Message = "Xóa Tài liệu thành công";
                return result;
            }
            result.Errors.Add("Xóa tài liệu thất bại");
            return result;
        }

        public async Task<DataResult<DocumentView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<DocumentView>();
            var document = await _unitOfWork.GetRepository<Document>().AsQueryable()
                .Include(x => x.User)
                .Include(x => x.Class)
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new DocumentView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    FileSize = x.FileSize,
                    FileExtension = x.FileExtension,
                    FilePath = x.FilePath,
                    Thumbnail = x.Thumbnail,
                    IsPublic = x.IsPublic,
                    Username = x.User.UserName,
                    ClassName = x.Class.Name,
                }).FirstOrDefaultAsync();
            if (document == null)
            {
                result.Errors.Add("Không tìm thấy tài liệu");
                return result;
            }
            result.Entity = document;
            return result;
        }

        public async Task<DataResult<DocumentView>> GetByClassPageList(BaseQuery<DocumentFilter> query)
        {
            var result = new DataResult<DocumentView>();
            // filter column not in table
            string usernameFilter = query?.Entity?.Username;
            string classId = query?.Entity?.ClassId;
            string className = query?.Entity?.ClassName;
            string subjectName = query?.Entity?.SubjectName;
            if (query.Entity != null && !string.IsNullOrEmpty(classId))
                query.Entity.ClassId = null;
            if (query.Entity != null && !string.IsNullOrEmpty(usernameFilter))
                query.Entity.Username = null;
            query.Entity.ClassName = null;
            query.Entity.SubjectName = null;


            var subjectIds = await _unitOfWork.GetRepository<Subject>().AsQueryable().Include(x => x.SubjectCourses).Where(x => x.SubjectCourses.Any(x => x.CourseId.ToString().Equals(query.Entity.CourseId))).Select(x => x.Id).ToListAsync();
            query.Entity.CourseId = null;
            var classQuery = _unitOfWork.GetRepository<Document>().AsQueryable()
                .Include(x => x.Class)
                .Include(x => x.Subject)
                .Include(x => x.User)
                .Where(x => x.ClassId.Equals(Guid.Parse(classId))
                            && (string.IsNullOrWhiteSpace(usernameFilter) || x.User.UserName.Contains(usernameFilter))
                            || subjectIds.Contains(x.SubjectId) &&
                            (string.IsNullOrWhiteSpace(className) || x.Class.Name.Contains(className))
                            &&
                            (string.IsNullOrWhiteSpace(subjectName) || x.Subject.Name.Contains(subjectName))
                            )
                .ApplyFilter(query)
                .OrderByColumns(query.SortColumns, query.SortOrder);
            // Apply filters
            if (query.SortColumns.Contains("className"))
            {
                classQuery = query.SortOrder == true ? classQuery.OrderBy(x => x.Class.Name) : classQuery = classQuery.OrderByDescending(x => x.Class.Name);
            }
            if (query.SortColumns.Contains("subjectName"))
            {
                classQuery = query.SortOrder == true ? classQuery.OrderBy(x => x.Subject.Name) : classQuery = classQuery.OrderByDescending(x => x.Subject.Name);
            }

            result.TotalRecords = await classQuery.CountAsync();
            result.Items = await classQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new DocumentView(x)
                    {
                        Name = x.Name,
                        Description = x.Description,
                        FileSize = x.FileSize,
                        FileExtension = x.FileExtension,
                        FilePath = x.FilePath,
                        Thumbnail = x.Thumbnail,
                        IsPublic = x.IsPublic,
                        Username = x.User.UserName,
                        ClassName = x.Class.Name,
                        SubjectName = x.Subject.Name
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<DocumentView>> GetByIdAsync(string id)
        {
            var result = new DataResult<DocumentView>();
            var document = await _unitOfWork.GetRepository<Document>().AsQueryable()
                .Include(x => x.User)
                .Include(x => x.Class)
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .Select(x => new DocumentView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    FileSize = x.FileSize,
                    FileExtension = x.FileExtension,
                    FilePath = x.FilePath,
                    Thumbnail = x.Thumbnail,
                    IsPublic = x.IsPublic,
                    Username = x.User.UserName,
                    ClassName = x.Class.Name,
                }).FirstOrDefaultAsync();
            if (document == null)
            {
                result.Errors.Add("Không tìm thấy tài liệu");
                return result;
            }
            result.Entity = document;
            return result;
        }

        public async Task<DataResult<DocumentView>> GetPageList(BaseQuery<DocumentFilter> query)
        {
            var result = new DataResult<DocumentView>();
            // filter column not in table
            string classNameFilter = query?.Entity?.ClassName;
            string usernameFilter = query?.Entity?.Username;
            string subjectNameFilter = query?.Entity?.SubjectName;
            if (query.Entity != null && !string.IsNullOrEmpty(classNameFilter))
                query.Entity.ClassName = null;
            if (query.Entity != null && !string.IsNullOrEmpty(usernameFilter))
                query.Entity.Username = null;     
            if (query.Entity != null && !string.IsNullOrEmpty(subjectNameFilter))
                query.Entity.SubjectName = null;


            var classQuery = _unitOfWork.GetRepository<Document>().AsQueryable()
                .Include(x => x.Class).Include(x => x.Subject).Include(x => x.User)
                .ApplyFilter(query)
                .OrderByColumns(query.SortColumns, query.SortOrder);
            if (!string.IsNullOrEmpty(classNameFilter))
                classQuery = classQuery.Where(x => x.Class.Name.Contains(classNameFilter));
            if (!string.IsNullOrEmpty(usernameFilter))
                classQuery = classQuery.Where(x => x.User.UserName.Contains(usernameFilter));
            if (!string.IsNullOrEmpty(subjectNameFilter))
                classQuery = classQuery.Where(x => x.Subject.Name.Contains(subjectNameFilter));
            if (query.SortColumns.Contains("className"))
            {
                classQuery = query.SortOrder == true ? classQuery.OrderBy(x => x.Class.Name) : classQuery = classQuery.OrderByDescending(x => x.Class.Name);
            }
            if (query.SortColumns.Contains("subjectName"))
            {
                classQuery = query.SortOrder == true ? classQuery.OrderBy(x => x.Subject.Name) : classQuery = classQuery.OrderByDescending(x => x.Subject.Name);
            }
            if (query.SortColumns.Contains("username"))
            {
                classQuery = query.SortOrder == true ? classQuery.OrderBy(x => x.User.UserName) : classQuery = classQuery.OrderByDescending(x => x.User.UserName);
            }
            result.TotalRecords = await classQuery.CountAsync();
            result.Items = await classQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new DocumentView(x)
                    {
                        Name = x.Name,
                        Description = x.Description,
                        FileSize = x.FileSize,
                        FileExtension = x.FileExtension,
                        FilePath = x.FilePath,
                        Thumbnail = x.Thumbnail,
                        IsPublic = x.IsPublic,
                        Username = x.User.UserName,
                        ClassName = x.Class.Name,
                        SubjectName = x.Subject.Name
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(DocumentQuery entity, string id)
        {
            var result = new DataResult<int>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<DocumentQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // userLogin
            var userLogin = await GetUserLoginAsync();
            if (userLogin == null)
            {
                result.Errors.Add("Vui lòng đăng nhập");
                return result;
            }
            // Update entity
            var documentRepository = _unitOfWork.GetRepository<Document>();
            var document = await documentRepository.AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (document == null)
            {
                result.Errors.Add("Không tìm thấy tài liệu");
                return result;
            }
            document.Name = entity.Name;
            document.Description = entity.Description;
            document.FileSize = entity.FileSize;
            document.FileExtension = entity.FileExtension;
            document.FilePath = entity.FilePath;
            document.Thumbnail = entity.Thumbnail;
            document.IsPublic = entity.IsPublic;
            document.ClassId = entity.ClassId;
            document.UpdatedBy = entity.UpdatedBy;
            document.UpdatedAt = DateTime.Now;
            documentRepository.Update(document);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Cập nhật tài liệu thành công";
                return result;
            }
            result.Errors.Add("Cập nhật tài liệu thất bại");
            return result;
        }

        public async Task<DataResult<DocumentView>> GetBySubjectPageList(BaseQuery<DocumentFilter> query)
        {
            var result = new DataResult<DocumentView>();
            // filter column not in table

            string usernameFilter = query?.Entity?.Username;
            string subjectCode = query?.Entity?.SubjectCode;
            if (query.Entity != null && !string.IsNullOrEmpty(subjectCode))
                query.Entity.SubjectCode = null;
            if (query.Entity != null && !string.IsNullOrEmpty(usernameFilter))
                query.Entity.Username = null;

            var subjectQuery = _unitOfWork.GetRepository<Document>().AsQueryable()
                .Include(x => x.Subject)
                .Include(x => x.User)
                .Where(x => x.Subject.Code == subjectCode
                            && (string.IsNullOrWhiteSpace(usernameFilter) || x.User.UserName.Contains(usernameFilter)))
                .ApplyFilter(query)
                .OrderByColumns(query.SortColumns, query.SortOrder);

            result.TotalRecords = await subjectQuery.CountAsync();
            result.Items = await subjectQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new DocumentView(x)
                    {
                        Name = x.Name,
                        Description = x.Description,
                        FileSize = x.FileSize,
                        FileExtension = x.FileExtension,
                        FilePath = x.FilePath,
                        Thumbnail = x.Thumbnail,
                        IsPublic = x.IsPublic,
                        Username = x.User.UserName,
                        SubjectName = x.Subject.Name,
                    }).ToListAsync();
            return result;
        }
    }
}