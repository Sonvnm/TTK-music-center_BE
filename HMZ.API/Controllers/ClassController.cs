using HMZ.API.Controllers.Base;
using HMZ.Database.Entities;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Services;
using HMZ.Service.Services.ClassServices;
using HMZ.Service.Services.DocumentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace HMZ.API.Controllers
{
    public class ClassController : CRUDController<IClassService, ClassQuery, ClassView, ClassFilter>
    {

        private readonly IDocumentService _documentService;
        private readonly IUnitOfWork _unitOfWork;
        public ClassController(IClassService service,IDocumentService documentService,IUnitOfWork unitOfWork) : base(service)
        {
            _documentService = documentService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public override async Task<IActionResult> GetAll(BaseQuery<ClassFilter> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var items = await _service.GetPageList(query);
            return Ok(items);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetStudentByClass(BaseQuery<StudentQuery> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;

            var result = await _service.GetStudentByClassPageList(query);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{isTeacher}")]
        public async Task<IActionResult> AddStudentToClass(StudentClassQuery query, bool isTeacher = false)
        {
            var result = await _service.AddStudentToClass(query, isTeacher);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RemoveStudentClass(StudentClassQuery query)
        {
            var result = await _service.RemoveStudentFromClass(query);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> GetStudentNotInClass(BaseQuery<StudentQuery> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;

            var result = await _service.GetStudentByPageList(query);
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> GetTeacherNotInClass(BaseQuery<StudentQuery> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;

            var result = await _service.GetStudentByPageList(query, true);
            return Ok(result);
        }


        [Authorize]
        [HttpGet("{userId}")]

        public async Task<IActionResult> GetClassesByUserId(string userId)
        {
            var result = await _service.GetClassesByUserId(userId);

            return Ok(result);
        }

        [Authorize]
        [HttpPost]

        public async Task<IActionResult> DeleteDocumentsFromClass(string[] documentsId)
        {
            var username = User.Identity.Name;
            var result = await _service.DeleteDocumentsFromClass(username, documentsId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{courseId}")]

        public async Task<IActionResult> GetClassesByCourse(string courseId)
        {
            var result = await _service.GetClassesByCourse(courseId);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("{classId}")]

        public async Task<IActionResult> GetStudentsClassByClassId(Guid classId)
        {
            var result = await _service.GetStudentsClassByClassId(classId);

            return Ok(result);
        }

    }
}