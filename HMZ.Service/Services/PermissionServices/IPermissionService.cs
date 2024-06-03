using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.PermissionServices
{
    public interface IPermissionService : IBaseService<PermissionQuery, PermissionView, PermissionFilter>
    {
        Task<DataResult<int>> AddToRolePermissionAsync(string roleCode, string[] permissionsId);
        Task<DataResult<int>> RemoveRolePermissionAsync(string roleName, string[] permissionsId);
        Task<DataResult<bool>> UpdateRolePermissionAsync(PermissionQuery entity, Guid roleId, Guid permissionId);

        // Get by role id
        Task<DataResult<RoleView>> GetByRoleAsync(Guid roleId);
        Task<DataResult<RoleView>> GetByRoleAsync(string roleName);
        Task<DataResult<PermissionView>> GetByRoleAsync(BaseQuery<PermissionFilter> filter);
        Task<DataResult<PermissionView>> GetNotInRoleAsync(BaseQuery<PermissionFilter> filter);
        // get by user 
        Task<DataResult<UserView>> GetByUserAsync(Guid userId);
        Task<DataResult<UserView>> GetByUserAsync(string username);

        // get all  role permissions
        Task<DataResult<PermissionView>> GetAllRolePermissionsAsync(BaseQuery<PermissionFilter> query);
    }
}
