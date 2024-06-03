
using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Services.ScheduleDetailServices;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class ScheduleDetailController : CRUDController<IScheduleDetailService, ScheduleDetailQuery, ScheduleDetailView, ScheduleDetailFilter>
    {
        public ScheduleDetailController(IScheduleDetailService service) : base(service)
        {




        }

        [HttpGet("{scheduleId}")]
        public async Task<IActionResult> GetSchedulesDetailByScheduleId(string scheduleId)
        {
            var result=await _service.GetSchedulesDetailByScheduleId(scheduleId);
            return Ok(result);
        }

        [HttpGet("{classId}")]
        public async Task<IActionResult> GetSchedulesDetailByClassId(Guid classId)
        {
            var result = await _service.GetSchedulesDetailByClassId(classId);
            return Ok(result);
        }

    }

}
