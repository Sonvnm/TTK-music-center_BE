
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.OrderServices
{
    public interface IOrderDetailService : IBaseService<OrderDetailQuery, OrderDetailView, OrderDetailFilter>
    {

    }
}