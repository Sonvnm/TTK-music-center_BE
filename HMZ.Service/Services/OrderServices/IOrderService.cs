
using HMZ.Database.Enums;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.OrderServices
{
    public interface IOrderService: IBaseService<OrderQuery, OrderView, OrderFilter>
    {
        public Task<DataResult<bool>> UpdateStatusAsync(Guid id, EOrderStatus status);

        public Task<DataResult<OrderView>> GetOrderByUserId(string userId);

        public Task<DataResult<bool>> CheckCourseIsOrdered(Guid courseId);

         // For Dashboard
        Task<DataResult<ChartView>> GetDashboardData();
    }
}