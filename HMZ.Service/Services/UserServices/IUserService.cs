using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;
namespace HMZ.Service.Services.UserServices
{
    public interface IUserService : IBaseService<UserQuery, UserView, UserFilter>
    {
        Task<DataResult<UserLoginView>> Register(RegisterQuery entity);
        Task<DataResult<UserLoginView>> Login(LoginQuery entity);
        Task<DataResult<UserLoginView>> LoginWithGoogle(ExternalAuth entity);
        Task<DataResult<UserLoginView>> LoginWithFacebook(ExternalAuth entity);
        Task<DataResult<UserView>> GetByUserName(string userName);
        Task<DataResult<bool>> ChangeRole(ChangeRoleQuery entity);
        Task<DataResult<bool>> UpdatePassword(UpdatePasswordQuery entity);
        Task<DataResult<bool>> UploadAvatar(UploadAvatarQuery entity);
        Task<DataResult<bool>> ForgotPassword(string email, string host);
        Task<DataResult<bool>> ResetPassword(UpdatePasswordQuery entity);
        Task<DataResult<bool>> LockUser(string username, bool isLock);
        Task<DataResult<bool>> ChangePassword(UpdatePasswordQuery query);

        // Payment Salary for Teacher
        Task<DataResult<CalculateSalaryView>> CalculateSalaryForTeacher(CalculateSalaryQuery query);
        Task<DataResult<bool>> PaymentSalaryForTeacher(CalculateSalaryQuery query);

        // For Dashboard
        Task<DataResult<ChartView>> GetDashboardData();
        Task<DataResult<int>> GetCountAllMember();

    }
}
