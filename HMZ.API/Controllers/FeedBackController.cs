using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Services.FeedBackServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class FeedBackController : CRUDController<IFeedBackService, FeedBackQuery, FeedBackView, FeedBackFilter>
    {
        public FeedBackController(IFeedBackService service) : base(service)
        {
        }

        [HttpPut("{feedBackId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid? feedBackId)
        {
            
            var result = await _service.Approve(1, feedBackId); // 1. Approve, 2. Cancel
            return Ok(result);
        }
        [HttpPut("{feedBackId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid? feedBackId)
        {
            var result = await _service.Approve(2, feedBackId); // 1. Approve, 2. Cancel
            return Ok(result);
        }
    }
}