
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
using HMZ.Service.MailServices;

namespace HMZ.Service.Services.FeedBackServices
{
    public class FeedBackService : ServiceBase<IUnitOfWork>, IFeedBackService
    {
        private IMailService _mailService;
        public FeedBackService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider, IMailService mailService) : base(unitOfWork, serviceProvider)
        {
            _mailService = mailService;
        }

        public async Task<DataResult<bool>> Approve(int type, Guid? feedBackId)
        {
            var result = new DataResult<bool>();
            var feedBack = await _unitOfWork.GetRepository<FeedBack>().AsQueryable()
                .Include(x => x.User)
                .Where(x => x.Id == feedBackId && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (feedBack == null)
            {
                result.Errors.Add("Không tìm thấy phản hồi");
                return result;
            }
            var userLogin = await GetUserLoginAsync();
            if (userLogin == null)
            {
                result.Errors.Add("Bạn chưa đăng nhập");
                return result;
            }

            feedBack.Status = type == 1 ? EFeedBackStatus.Done : EFeedBackStatus.Canceled;
            feedBack.UpdatedAt = DateTime.Now;
            feedBack.UpdatedBy = userLogin.UserName;

            _unitOfWork.GetRepository<FeedBack>().Update(feedBack);
            if ( _unitOfWork.SaveChanges() > 0)
            {
                // send mail
                var mail = new MailQuery()
                {
                    Subject = type == 1 ? "[Thông báo] Phản hồi của bạn đã được duyệt" : "[Thông báo] Phản hồi của bạn đã bị hủy",
                    Body = "Cảm ơn bạn đã phản hồi với chúng tôi",
                    ToEmails = new List<string>() { feedBack.User.Email }
                };
                await _mailService.SendEmailAsync(mail);
                result.Message = "Phản hồi đã được duyệt";
                return result;
            }
            result.Errors.Add("Duyệt phản hồi thất bại");
            return result;
        }

        public async Task<DataResult<bool>> CreateAsync(FeedBackQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<FeedBackQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Check user
            var userLogin = await GetUserLoginAsync();
            if (userLogin == null)
            {
                result.Errors.Add("Bạn chưa đăng nhập");
                return result;
            }
            // check is exist in day
            var isExist = await _unitOfWork.GetRepository<FeedBack>().AsQueryable()
                .Where(x => x.UserId == userLogin.Id && x.CreatedAt.Value.Date == DateTime.Now.Date)
                .FirstOrDefaultAsync();
            if (isExist != null)
            {
                result.Errors.Add("Bạn đã phản hồi trong ngày");
                return result;
            }

            // Create entity
            var feedBack = new FeedBack
            {
                Title = entity.Title,
                Description = entity.Description,
                UserId = userLogin.Id,
                Type = entity.Type,
                Status = EFeedBackStatus.New,
                CreatedBy = entity.CreatedBy,
            };
            await _unitOfWork.GetRepository<FeedBack>().Add(feedBack);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Đã phản hồi";
                return result;
            }
            result.Errors.Add("Lỗi phản hồi");
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
            var roomRepository = _unitOfWork.GetRepository<FeedBack>();
            var rooms = await roomRepository.AsQueryable()
                .Where(x => id.Contains(x.Id.ToString()))
                .ToListAsync();

            if (rooms == null || rooms.Count == 0)
            {
                result.Errors.Add("Không tìm thấy dữ liệu");
                return result;
            }
            roomRepository.DeleteRange(rooms); // false: remove from db, true: set IsActive = false
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa phản hồi thành công";
                return result;
            }
            result.Errors.Add("Xóa phản hồi thất bại");
            return result;
        }

        public async Task<DataResult<FeedBackView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<FeedBackView>();
            if (string.IsNullOrEmpty(code))
            {
                result.Errors.Add("Code is null or empty");
                return result;
            }
            var feedBackView = await _unitOfWork.GetRepository<FeedBack>().AsQueryable()
                .Include(x => x.User)
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new FeedBackView(x)
                {
                    Title = x.Title,
                    Description = x.Description,
                    Type = x.Type.ToString(),
                    Status = x.Status.ToString(),
                    UserId = x.UserId,
                    Username = x.User.UserName,
                })
                .FirstOrDefaultAsync();
            if (feedBackView == null)
            {
                result.Errors.Add("Không tìm thấy phản hồi");
                return result;
            }
            result.Entity = feedBackView;
            return result;
        }

        public async Task<DataResult<FeedBackView>> GetByIdAsync(string id)
        {
            var result = new DataResult<FeedBackView>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var feedBackView = await _unitOfWork.GetRepository<FeedBack>().AsQueryable()
                .Include(x => x.User)
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .Select(x => new FeedBackView(x)
                {
                    Title = x.Title,
                    Type = x.Type.ToString(),
                    Status = x.Status.ToString(),
                    Description = x.Description,
                    UserId = x.UserId,
                    Username = x.User.UserName,
                })
                .FirstOrDefaultAsync();
            if (feedBackView == null)
            {
                result.Errors.Add("Không tìm thấy phản hồi");
                return result;
            }
            result.Entity = feedBackView;
            return result;
        }

        public async Task<DataResult<FeedBackView>> GetPageList(BaseQuery<FeedBackFilter> query)
        {
            var result = new DataResult<FeedBackView>();
            string username = string.IsNullOrEmpty(query.Entity?.Username) ? null : query.Entity.Username;
            if (query.Entity != null)
            {
                query.Entity.Username = null;
            }

            var roomQuery = _unitOfWork.GetRepository<FeedBack>().AsQueryable()
                        .Include(x => x.User)
                        .ApplyFilter(query)
                        .WhereIf(!string.IsNullOrEmpty(username), x => x.User.UserName.Contains(username))
                        .OrderByColumns(query.SortColumns, query.SortOrder);
            result.TotalRecords = await roomQuery.CountAsync();
            result.Items = await roomQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new FeedBackView(x)
                    {
                        Title = x.Title,
                        Status = x.Status.ToString(),
                        Type = x.Type.ToString(),
                        Username = x.User.UserName,
                        Description = x.Description,
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(FeedBackQuery entity, string id)
        {
            var result = new DataResult<int>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var feedBack = await _unitOfWork.GetRepository<FeedBack>().AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (feedBack == null)
            {
                result.Errors.Add("Không tìm thấy phản hồi");
                return result;
            }

            feedBack.Title = entity.Title;
            feedBack.Description = entity.Description;
            feedBack.Type = entity.Type;
            feedBack.Status = entity.Status;

            feedBack.UpdatedBy = entity.UpdatedBy;
            feedBack.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<FeedBack>().Update(feedBack);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Cập nhật phản hồi thành công";
                return result;
            }
            result.Errors.Add("Cập nhật phản hồi thất bại");
            return result;
        }
    }
}