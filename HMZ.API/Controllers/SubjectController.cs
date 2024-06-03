using HMZ.API.Controllers.Base;
using HMZ.Database.Entities;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Services.SubjectServices;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class SubjectController : CRUDController<ISubjectService, SubjectQuery, SubjectView, SubjectFilter>
    {
        public SubjectController(ISubjectService service) : base(service)
        {
        }


        [HttpPost("{courseId}")]

        public async Task<IActionResult> GetSubjectsForCourse(BaseQuery<SubjectFilter> query,string courseId)
        {
            var result = await _service.GetSubjectsForCourse(query,courseId);
            return Ok(result);
        }


    }
}