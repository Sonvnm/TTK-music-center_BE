using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.SubjectServices
{
    public interface ISubjectService : IBaseService<SubjectQuery, SubjectView, SubjectFilter>
    {
        public Task<DataResult<SubjectView>> GetSubjectsForCourse(BaseQuery<SubjectFilter> query,string courseId);
    }
}