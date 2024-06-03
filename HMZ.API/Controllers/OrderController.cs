using HMZ.API.Controllers.Base;
using HMZ.Database.Enums;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.OrderServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HMZ.API.Controllers
{

    public class OrderController : CRUDController<IOrderService, OrderQuery, OrderView, OrderFilter>
    {
        public OrderController(IOrderService service) : base(service)
        {

        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public override async Task<IActionResult> Delete(string[] id)
        {
            if (id == null || id.Length == 0)
            {
                return Ok(new DataResult<bool> { Entity = false, Errors = new List<string> { "Id is required" } });
            }
            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }

        [HttpPut("id")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] EOrderStatus status)
        {
            var result = await _service.UpdateStatusAsync(id, status);
            return Ok(result);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetOrderByUserId(string userId)
        {
            var result = await _service.GetOrderByUserId(userId);
            return Ok(result);
        }

        [HttpGet("{courseId}")]
        public async Task<IActionResult> CheckCourseIsOrdered(Guid courseId)
        {
            var result = await _service.CheckCourseIsOrdered(courseId);
            return Ok(result);
        }
    }
}