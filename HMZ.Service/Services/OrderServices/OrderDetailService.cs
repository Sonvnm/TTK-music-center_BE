
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.OrderServices
{
    public class OrderDetailService : ServiceBase<IUnitOfWork>, IOrderDetailService
    {
        ///  Không dùng service này vì đã có service OrderService

        /// <summary>
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="serviceProvider"></param> <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public OrderDetailService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(unitOfWork, serviceProvider)
        {
        }

        public Task<DataResult<bool>> CreateAsync(OrderDetailQuery entity)
        {
            throw new NotImplementedException();
        }

        public Task<DataResult<int>> DeleteAsync(string[] id)
        {
            throw new NotImplementedException();
        }

        public Task<DataResult<OrderDetailView>> GetByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<DataResult<OrderDetailView>> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<DataResult<OrderDetailView>> GetPageList(BaseQuery<OrderDetailFilter> query)
        {
            throw new NotImplementedException();
        }

        public Task<DataResult<int>> UpdateAsync(OrderDetailQuery entity, string id)
        {
            throw new NotImplementedException();
        }
    }
}