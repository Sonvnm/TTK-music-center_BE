
using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Services.CourseServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class CourseController : CRUDController<ICourseService, CourseQuery, CourseView, CourseFilter>
    {
        public CourseController(ICourseService service) : base(service)
        {
        }

        // Get Subject by Course
        [HttpPost("subjects")]
        public virtual async Task<IActionResult> GetAll(BaseQuery<SubjectQuery> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var items = await _service.GetSubjectByCoursePageList(query);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubjectToCourse(SubjectCourseQuery query)
        {
            var result = await _service.AddSubjectToCourse(query);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]

        public async Task<IActionResult> RemoveSubjectCourse(SubjectCourseQuery query)
        {
            var result = await _service.RemoveSubjectCourse(query);
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public override async Task<IActionResult> Create([FromForm] CourseQuery query)
        {
            var result = await _service.CreateAsync(query);
            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]

        public override async Task<IActionResult> Update(string id, [FromForm] CourseQuery query)
        {
            var result = await _service.UpdateAsync(query, id);
            return Ok(result);
        }


        
        [HttpGet]
        public async Task<IActionResult> GetAllForStudent()
        {
            var result = await _service.GetAllForStudent();
            return Ok(result);
        }


        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetSubjectCourseById(string courseId)
        {
            var userName = User.Identity.Name;
            var result = await _service.GetSubjectCourseById(courseId,userName);
            return Ok(result);
        }





    }
}