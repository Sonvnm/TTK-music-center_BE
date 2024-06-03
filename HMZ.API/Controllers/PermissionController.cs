using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Services.PermissionServices;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class PermissionController : CRUDController<IPermissionService, PermissionQuery, PermissionView, PermissionFilter>
    {
        public PermissionController(IPermissionService service) : base(service)
        {
        }

        #region  Permission Role CRUD

        [HttpPost]
        public async Task<IActionResult> GetAllRolePermissions(BaseQuery<PermissionFilter> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var permissions = await _service.GetAllRolePermissionsAsync(query);
            return  Ok(permissions);
        }
        // get all permissions by user id
        [HttpPost("{id}")]
        public async Task<IActionResult> GetPermissionsByUserId(string id)
        {
            var result = await _service.GetByUserAsync(id);
            return Ok(result);
        }
        [HttpPost("{username}")]
        public async Task<IActionResult> GetPermissionsByUsername(string username)
        {
            var result = await _service.GetByUserAsync(username);
            return  Ok(result);
        }
        // get all permissions by role id
        [HttpPost("{id}")]
        public async Task<IActionResult> GetPermissionsByRoleId(string id)
        {
            var result = await _service.GetByRoleAsync(id);
            return  Ok(result);
        }
        [HttpGet("{roleName}")]
        public async Task<IActionResult> GetPermissionsByRoleName(string roleName)
        {
            var result = await _service.GetByRoleAsync(roleName);
            return Ok(result);
        }
        // get all permissions by role name
        [HttpPost]
        public async Task<IActionResult> GetByRole(BaseQuery<PermissionFilter> query)
        {
             query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
             query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var result = await _service.GetByRoleAsync(query);
            return Ok(result);
        }
        // get all permissions not in role
        [HttpPost]
        public async Task<IActionResult> GetNotInRole(BaseQuery<PermissionFilter> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var result = await _service.GetNotInRoleAsync(query);
            return Ok(result);
        }

        [HttpPost("{roleCode}")]
        public async Task<IActionResult> AddPermissionsToRole(string roleCode, string[] permissionsId)
        {
            var result = await _service.AddToRolePermissionAsync(roleCode, permissionsId);
            return Ok(result);
        }
        [HttpPost("{roleCode}")]
        public async Task<IActionResult> RemoveRolePermission(string roleCode, string[] permissionsId)
        {
            var result = await _service.RemoveRolePermissionAsync(roleCode, permissionsId);
            return Ok(result);
        }
        [HttpPut("{roleId}/{permissionId}")]
        public async Task<IActionResult> UpdateRolePermission(string roleId, string permissionId, PermissionQuery permissionQuery)
        {
            var result = await _service.UpdateRolePermissionAsync(permissionQuery, Guid.Parse(roleId), Guid.Parse(permissionId));
            return Ok(result);
        }
        #endregion

    }
}