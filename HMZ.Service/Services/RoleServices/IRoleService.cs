using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.RoleServices
{
    public interface IRoleService : IBaseService<RoleQuery, RoleView, RoleFilter>
    {
        public Task<RoleView> GetByNameAsync(string name); 

		//  #region  UserRoles
		public Task<DataResult<int>> AddUserToRoleAsync(string username, string roleName);
        public Task<DataResult<int>> RemoveUserFromRoleAsync(string username, string roleName);
        public Task<DataResult<RoleView>> GetRolesByUsernameAsync(string username);
    }
}