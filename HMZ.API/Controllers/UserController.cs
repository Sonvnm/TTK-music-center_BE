using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.SDK.Excel;
using HMZ.Service.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{

    //[Authorize(Policy = "Admin")]
    public class UserController : CRUDController<IUserService, UserQuery, UserView, UserFilter>
    {
        public UserController(IUserService service) : base(service)
        {
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel(BaseQuery<UserFilter> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var result = await _service.GetPageList(query);
            if (result.Success == false)
            {
                return BadRequest(result);
            }
            var exportExcel = new ExcelExportBase<UserView>();
            var exportResult = await exportExcel.ExportToExcel(result.Items, "Export_User.xlsx");
            if (exportResult.ErrorMessage != null)
            {
                return BadRequest(exportResult.ErrorMessage);
            }
            return new FileContentResult(exportResult.Content, exportResult.ContentType)
            {
                FileDownloadName = exportResult.FileName
            };

        }

        // Forgot password
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordQuery query)
        {
            if (query.Email == null || query.Host == null)
                return Ok(new { Success = false, Message = "Email is null" });
            var result = await _service.ForgotPassword(query.Email, query.Host);
            return Ok(result);
        }
        // Reset password
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(UpdatePasswordQuery? entity)
        {
            var result = await _service.ResetPassword(entity);
            return Ok(result);
        }

        [HttpPost]

        public async Task<IActionResult> ChangePassword(UpdatePasswordQuery entity)
        {
            var result = await _service.UpdatePassword(entity);
            return Ok(result);
        }

        // lock user
        [HttpPost]
        public async Task<IActionResult> LockUser(string username, bool isLock)
        {
            var result = await _service.LockUser(username, isLock);
            return Ok(result);
        }

        // get user by username
        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var result = await _service.GetByUserName(username);
            return Ok(result);
        }

        // change password
        [HttpPut]
        public async Task<IActionResult> ChangePasswordUser(UpdatePasswordQuery query)
        {
            var result = await _service.ChangePassword(query);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMember()
        {
            var result = await _service.GetCountAllMember();
            return Ok(result);
        }
    }
}
