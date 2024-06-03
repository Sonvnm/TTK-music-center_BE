using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.StudentStudyProcessServices
{
    public interface IStudentStudyProcessService : IBaseService<StudentStudyProcessQuery, StudentStudyProcessView, StudentStudyProcessFilter>
    {
        Task<DataResult<StudentStudyProcessView>> GetByLearningProcessPageList(BaseQuery<StudentStudyProcessFilter> query,Guid learningProcessId);
        Task<DataResult<StudentStudyProcessView>> GetLearningProcessByClassId(BaseQuery<StudentStudyProcessFilter> query,string classId,string userName);
        Task<DataResult<bool>> UpdateProcessStudent(StudentStudyProcessQuery query,string id,string userName);

        Task<DataResult<bool>> CreateStudyProcesses(List<StudentStudyProcessQuery> query, string userName);
    }
}