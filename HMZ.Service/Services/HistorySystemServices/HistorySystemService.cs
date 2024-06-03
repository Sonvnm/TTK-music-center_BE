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

namespace HMZ.Service.Services.HistorySystemServices
{
    public class HistorySystemService : ServiceBase<IUnitOfWork>, IHistorySystemService
    {
        public HistorySystemService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(unitOfWork, serviceProvider)
        {
        }

        public async Task<DataResult<bool>> CreateAsync(HistorySystemQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<HistorySystemQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Create entity
            var historySystem = new HistorySystem
            {
                Action = entity.Action,
                Description = entity.Description,
                Type = entity.Type,
                UserId = entity.UserId,
                CreatedBy = entity.CreatedBy,
            };
            await _unitOfWork.GetRepository<HistorySystem>().Add(historySystem);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Đã log lịch sử";
                return result;
            }
            result.Errors.Add("Lỗi khi log lịch sử");
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
            var roomRepository = _unitOfWork.GetRepository<HistorySystem>();
            var rooms = await roomRepository.AsQueryable()
                .Where(x => id.Contains(x.Id.ToString()))
                .ToListAsync();

            if (rooms == null || rooms.Count == 0)
            {
                result.Errors.Add("Log lịch sử không tồn tại");
                return result;
            }
            roomRepository.DeleteRange(rooms); // false: remove from db, true: set IsActive = false
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa Lịch sử thành công";
                return result;
            }
            result.Errors.Add("Xóa lịch sử thất bại");
            return result;
        }

        public async Task<DataResult<HistorySystemView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<HistorySystemView>();
            if (string.IsNullOrEmpty(code))
            {
                result.Errors.Add("Code is null or empty");
                return result;
            }
            var historySystemView = await _unitOfWork.GetRepository<HistorySystem>().AsQueryable()
                .Include(x => x.User)
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new HistorySystemView(x)
                {
                    Action = x.Action,
                    Description = x.Description,
                    Type = x.Type.ToString(),
                    Username = x.User.UserName,
                })
                .FirstOrDefaultAsync();
            if (historySystemView == null)
            {
                result.Errors.Add("Không tìm thấy phòng học");
                return result;
            }
            result.Entity = historySystemView;
            return result;
        }

        public async Task<DataResult<HistorySystemView>> GetByIdAsync(string id)
        {
            var result = new DataResult<HistorySystemView>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var historySystemView = await _unitOfWork.GetRepository<HistorySystem>().AsQueryable()
                .Include(x => x.User)
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .Select(x => new HistorySystemView(x)
                {
                    Action = x.Action,
                    Type = x.Type.ToString(),
                    Description = x.Description,
                    Username = x.User.UserName,
                })
                .FirstOrDefaultAsync();
            if (historySystemView == null)
            {
                result.Errors.Add("Không tìm thấy phòng học");
                return result;
            }
            result.Entity = historySystemView;
            return result;
        }

        public async Task<DataResult<HistorySystemView>> GetPageList(BaseQuery<HistorySystemFilter> query)
        {
            var result = new DataResult<HistorySystemView>();
            string username = query.Entity?.Username ?? string.Empty;
           if(query.Entity != null)
            {
                query.Entity.Username = null;
            }
            
            var roomQuery = _unitOfWork.GetRepository<HistorySystem>().AsQueryable()
                            .Include(x => x.User)
                            .ApplyFilter(query)
                            .WhereIf(!string.IsNullOrEmpty(username), x=> x.User.UserName.Contains(username))
                            .OrderByColumns(query.SortColumns, query.SortOrder);
                            
            result.TotalRecords = await roomQuery.CountAsync();
            result.Items = await roomQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new HistorySystemView(x)
                    {
                        Action = x.Action,
                        Type = x.Type.ToString(),
                        Description = x.Description,
                        Price=x.Price,

                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(HistorySystemQuery entity, string id)
        {
            var result = new DataResult<int>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var historySystem = await _unitOfWork.GetRepository<HistorySystem>().AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (historySystem == null)
            {
                result.Errors.Add("Không tìm thấy phòng học");
                return result;
            }
            historySystem.Action = entity.Action;
            historySystem.Type = entity.Type;
            historySystem.Description = entity.Description;
            historySystem.UpdatedBy = entity.UpdatedBy;
            historySystem.UpdatedAt = DateTime.Now;
            
            _unitOfWork.GetRepository<HistorySystem>().Update(historySystem);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Cập nhật lịch sử thành công";
                return result;
            }
            result.Errors.Add("Cập nhật lịch sử thất bại");
            return result;
        }
    }
}