using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.OrderServices;

namespace HMZ.Service.Validator
{
    public class OrderDetailValidator : IValidator<OrderDetailQuery>
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        public OrderDetailValidator(IOrderService orderService, IOrderDetailService orderDetailService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
        }

        public async Task<List<ValidationResult>> ValidateAsync(OrderDetailQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
            // check exist
            if (isUpdate != true)
            {
                var orderDetail = await _orderDetailService.GetByIdAsync(entity.Id.ToString());
                if (orderDetail == null)
                {
                    return new List<ValidationResult>(){
                        new ValidationResult("Chi tiết đơn hàng không tồn tại", new[] { nameof(entity.Id) })
                    };
                }
            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.CourseId), entity.CourseId),
            };
            return await Task.FromResult(result);
        }
    }
}