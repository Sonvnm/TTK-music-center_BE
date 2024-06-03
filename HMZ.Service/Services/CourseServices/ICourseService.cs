
using HMZ.Database.Entities;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.CourseServices
{
    public interface ICourseService : IBaseService<CourseQuery, CourseView, CourseFilter>
    {
        Task<DataResult<CourseView>> GetSubjectByCoursePageList(BaseQuery<SubjectQuery> query);

        Task<DataResult<int>> AddSubjectToCourse(SubjectCourseQuery query);

        Task<DataResult<bool>> RemoveSubjectCourse(SubjectCourseQuery query);

        Task<DataResult<SubjectCourseView>> GetAllForStudent();
        Task<DataResult<SubjectCourseView>> GetSubjectCourseById(string courseId,string userName);
    }
}