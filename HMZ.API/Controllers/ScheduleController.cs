using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Services.ScheduleServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class ScheduleController : CRUDController<IScheduleService, ScheduleQuery, ScheduleView, ScheduleFilter>
    {
        public ScheduleController(IScheduleService service) : base(service)
        {
        }


        // [HttpPost]
        // [Authorize]
        // public async Task<IActionResult> GetSchedulesByUser(BaseQuery<ScheduleFilter> filter)
        // {
        //     var username = User.Identity.Name;
        //     filter.PageNumber = filter.PageNumber > 0 ? filter.PageNumber : 1;
        //     filter.PageSize = filter.PageSize > 0 ? filter.PageSize : 10;
        //     var result = await _service.GetSchedulesByUser(filter, username);
        //     return Ok(result);
        // }

        // [HttpGet]
        // [Authorize]
        // public async Task<IActionResult> GetClassesRoomsForSchedule()
        // {
        //     var result = await _service.GetClassesRoomsForSchedule();
        //     return Ok(result);

        // }

        // [Authorize(Roles = "Admin,Teacher")]
        // [HttpPost]
        // public async Task<IActionResult> ModifySchedules(List<ScheduleQuery> query)
        // {
        //     var username = User.Identity.Name;
        //     var result = await _service.ModifySchedules(query, username);
        //     return Ok(result);

        // }


        // [Authorize(Roles = "Admin,Teacher")]
        // [HttpPost("{id}")]
        // public async Task<IActionResult> SendMail(string id)
        // {
        //     var username = User.Identity.Name;
        //     var result = await _service.SendMail(id);
        //     return Ok(result);

        // }

        [HttpGet]

        public async Task<IActionResult> GetSchedulesByUser()
        {
            var userName = User.Identity.Name;
            var result = await _service.GetSchedulesByUser(userName);
            return Ok(result);
        }
    }
}
