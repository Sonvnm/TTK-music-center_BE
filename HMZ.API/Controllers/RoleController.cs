using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Services.RoleServices;
using Microsoft.AspNetCore.Mvc;
namespace HMZ.API.Controllers
{
    public class RoleController : CRUDController<IRoleService,RoleQuery,RoleView,RoleFilter>
    {
        public RoleController(IRoleService service) : base(service)
        {

        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _service.GetByNameAsync(name);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> AddUserToRole(string username, string roleName)
        {
            var result = await _service.AddUserToRoleAsync(username, roleName);
            return Ok(result);
        }
        [HttpDelete]
        public async Task<IActionResult> RemoveUserFromRole(string username, string roleName)
        {
            var result = await _service.RemoveUserFromRoleAsync(username, roleName);
            return Ok(result);
        }
        [HttpGet("{username}")]
        public async Task<IActionResult> GetRolesByUsername(string username)
        {
            var result = await _service.GetRolesByUsernameAsync(username);
            return Ok(result);
        }
	}
    
}
