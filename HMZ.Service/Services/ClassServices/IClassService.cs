using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;
using NPOI.Util;

namespace HMZ.Service.Services.ClassServices
{
    public interface IClassService : IBaseService<ClassQuery, ClassView, ClassFilter>
    {
        Task<DataResult<UserView>> GetStudentByClassPageList(BaseQuery<StudentQuery> query);
        Task<DataResult<UserView>> GetStudentByPageList(BaseQuery<StudentQuery> query, bool isTeacher = false);
        Task<DataResult<int>> AddStudentToClass(StudentClassQuery query, bool isTeacher = false);
        Task<DataResult<int>> RemoveStudentFromClass(StudentClassQuery query);
        Task<DataResult<ClassView>> GetClassesByUserId(string userId);
        Task<DataResult<int>> DeleteDocumentsFromClass(string userName, string[] documentIds);
        Task<DataResult<ClassView>> GetClassesByCourse(string courseId);

        Task<DataResult<ClassView>> GetStudentsClassByClassId(Guid classId);

    }
}