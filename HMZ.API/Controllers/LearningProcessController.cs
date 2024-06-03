
using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Services.LearningProcessServices;
using Microsoft.AspNetCore.Mvc;
namespace HMZ.API.Controllers
{

    public class LearningProcessController : CRUDController<ILearningProcessService, LearningProcessQuery, LearningProcessView, LearningProcessFilter>
    {
        public LearningProcessController(ILearningProcessService service) : base(service)
        {
            
        }

        [HttpGet]

        public async Task<IActionResult> GetByUser()
        {
            var result = await _service.GetByUsername(User.Identity.Name);
            return Ok(result);
        }
        [HttpGet("{userName}")]

        public async Task<IActionResult> GetByUsername(string userName)
        {
            var result = await _service.GetByUsername(userName);
            return Ok(result);
        }
    }
}