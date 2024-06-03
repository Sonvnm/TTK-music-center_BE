using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Services.IBaseService;
namespace HMZ.Service.Services.RoomServices
{
    public interface IRoomService : IBaseService<RoomQuery, RoomView, RoomFilter>
    {
        
    }
}