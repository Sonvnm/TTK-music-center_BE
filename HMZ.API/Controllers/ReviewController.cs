using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.ReviewService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class ReviewController : CRUDController<IReviewService, ReviewQuery, ReviewView, ReviewFilter>
    {
        public ReviewController(IReviewService service) : base(service)
        {


        }

        [HttpGet("{courseId}")]

        public async Task<IActionResult> GetReviewsByCourse(Guid courseId)
        {
            var result =await _service.GetReviewsByCourse(courseId);
            return Ok(result);
        }


    }
}
