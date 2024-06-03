
using System.ComponentModel.DataAnnotations;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.OrderServices;

namespace HMZ.Service.Validator
{
    public class OrderValidator : IValidator<OrderQuery>
    {
        private readonly IOrderService _orderService;
        public OrderValidator(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(OrderQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.Name), entity.Name),
            };
            return await Task.FromResult(result);
        }
    }
}