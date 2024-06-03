using System.ComponentModel.DataAnnotations;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.RoomServices;
namespace HMZ.Service.Validator
{
    public class RoomValidator : IValidator<RoomQuery>
    {
        private readonly IRoomService _roomService;
        public RoomValidator(IRoomService roomService)
        {
            _roomService = roomService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(RoomQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
            // update
            if (isUpdate == true)
            {
                var room = await _roomService.GetByIdAsync(entity.Id.ToString());
                if (room.Entity == null)
                {
                    return new List<ValidationResult>(){
                        new ValidationResult("Không tìm thấy phòng học", new[] { nameof(entity.Id) })
                    };
                }
            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.Name), entity.Name),
            };
            return await Task.FromResult(result);
        }
    }
}