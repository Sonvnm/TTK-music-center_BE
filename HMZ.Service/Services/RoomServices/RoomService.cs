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
namespace HMZ.Service.Services.RoomServices
{
    public class RoomService : ServiceBase<IUnitOfWork>, IRoomService
    {
        public RoomService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(unitOfWork, serviceProvider)
        {
        }

        public async Task<DataResult<bool>> CreateAsync(RoomQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<RoomQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }

            var roomNameExist = await _unitOfWork.GetRepository<Room>().AsQueryable().FirstOrDefaultAsync(x => x.Name.Equals(entity.Name));
            if (roomNameExist != default)
            {
                {
                    result.Errors.Add("Tên phòng học này đã tồn tại !");
                    return result;
                }
            }

            // Create entity

            var room = new Room
            {
                Name = entity.Name,
                Description = entity.Description,
                CreatedBy = entity.CreatedBy,
            };
            await _unitOfWork.GetRepository<Room>().Add(room);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                return result;
            }
            result.Errors.Add("Thêm phòng học thất bại");
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
            var roomRepository = _unitOfWork.GetRepository<Room>();
            var learingProcessRepository = _unitOfWork.GetRepository<LearningProcess>();
            var listLearningProcess = await _unitOfWork.GetRepository<LearningProcess>().AsQueryable().Include(x => x.ScheduleDetail).ToListAsync();
            var rooms = await roomRepository.AsQueryable()
                .Where(x => id.Contains(x.Id.ToString()) && x.IsActive == true)
                .ToListAsync();
            if (rooms == null || rooms.Count == 0)
            {
                result.Errors.Add("Không tìm thấy phòng học");
                return result;
            }
            var learningProcesses = listLearningProcess.Where(x => id.Contains(x.ScheduleDetail?.RoomId.ToString() ?? "")).ToList();
            if (learningProcesses.Any(x=>x.Status.Equals(ELearningProcessStatus.Done.ToString())))
            {
                result.Errors.Add("Phòng học đã được sử dụng không thể xóa");
                return result;
            }
            else
            {
                learingProcessRepository.DeleteRange(learningProcesses);
            }
            roomRepository.DeleteRange(rooms);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa phòng học thành công";
                return result;
            }
            result.Errors.Add("Xóa phòng học thất bại");
            return result;
        }

        public async Task<DataResult<RoomView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<RoomView>();
            if (string.IsNullOrEmpty(code))
            {
                result.Errors.Add("Code is null or empty");
                return result;
            }
            var room = await _unitOfWork.GetRepository<Room>().AsQueryable()
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new RoomView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                })
                .FirstOrDefaultAsync();
            if (room == null)
            {
                result.Errors.Add("Không tìm thấy phòng học");
                return result;
            }
            result.Entity = room;
            return result;
        }

        public async Task<DataResult<RoomView>> GetByIdAsync(string id)
        {
            var result = new DataResult<RoomView>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var room = await _unitOfWork.GetRepository<Room>().AsQueryable()
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .Select(x => new RoomView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                })
                .FirstOrDefaultAsync();
            if (room == null)
            {
                result.Errors.Add("Không tìm thấy phòng học");
                return result;
            }
            result.Entity = room;
            return result;
        }

        public async Task<DataResult<RoomView>> GetPageList(BaseQuery<RoomFilter> query)
        {
            var result = new DataResult<RoomView>();
            var roomQuery = _unitOfWork.GetRepository<Room>().AsQueryable()
                .Where(x => x.IsActive == true)
                .ApplyFilter(query)
                .OrderByColumns(query.SortColumns, query.SortOrder);

            result.TotalRecords = await roomQuery.CountAsync();
            result.Items = await roomQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new RoomView(x)
                    {
                        Name = x.Name,
                        Description = x.Description,
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(RoomQuery entity, string id)
        {
            var result = new DataResult<int>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var roomRepository = _unitOfWork.GetRepository<Room>();
            var room = await roomRepository.GetByIdAsync(Guid.Parse(id));
            if (room == null)
            {
                result.Errors.Add("Không tìm thấy phòng học");
                return result;
            }
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<RoomQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity, isUpdate: true);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            var roomNameExist = await _unitOfWork.GetRepository<Room>().AsQueryable().FirstOrDefaultAsync(x => x.Name.Equals(entity.Name));
            if (roomNameExist != default && roomNameExist.Name != room.Name)
            {
                {
                    result.Errors.Add("Tên phòng học này đã tồn tại !");
                    return result;
                }
            }
            // Update entity
            room.Name = entity.Name;
            room.Description = entity.Description;
            room.UpdatedBy = entity.UpdatedBy;
            room.UpdatedAt = DateTime.Now;
            roomRepository.Update(room);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                return result;
            }
            result.Errors.Add("Cập nhật phòng học thất bại");
            return result;
        }
    }
}