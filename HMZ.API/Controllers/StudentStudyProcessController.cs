
using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Services.StudentStudyProcessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class StudentStudyProcessController : CRUDController<IStudentStudyProcessService, StudentStudyProcessQuery, StudentStudyProcessView, StudentStudyProcessFilter>
    {
        public StudentStudyProcessController(IStudentStudyProcessService service) : base(service)
        {
        }

        [HttpPost]
        public async  Task<IActionResult> CreateStudyProcesses(List<StudentStudyProcessQuery> query) 
        {
            var result = await _service.CreateStudyProcesses(query,User.Identity.Name);
            return Ok(result);
        }

        [HttpPost("{learningProcessId}")]
        [Authorize]

        public async Task<IActionResult> GetByLearningProcess(BaseQuery<StudentStudyProcessFilter> query, Guid learningProcessId)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var items = await _service.GetByLearningProcessPageList(query, learningProcessId);
            return Ok(items);
        }  
        
        [HttpPost("{classId}")]
        [Authorize]
        public async Task<IActionResult> GetLearningProcessByClassId(BaseQuery<StudentStudyProcessFilter> query, string classId)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var items = await _service.GetLearningProcessByClassId(query, classId, User.Identity.Name);
            return Ok(items);
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProcessStudent(StudentStudyProcessQuery query, string id)
        => Ok(await _service.UpdateProcessStudent(query, id,User.Identity.Name));
    }
}