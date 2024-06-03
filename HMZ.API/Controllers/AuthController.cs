using HMZ.API.Controllers.Base;
using HMZ.DTOs.Queries;
using HMZ.Service.Services.UserServices;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class AuthController : BaseController<IUserService>
    {
        public AuthController(IUserService service) : base(service)
        {
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginQuery user)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Login(user);
                return Ok(result);
            }
            return Error(ModelState.Values.First().Errors.First().ErrorMessage);

        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterQuery user)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Register(user);
                return Ok(result);
            }
            return Error(ModelState.Values.First().Errors.First().ErrorMessage);
        }
        [HttpPost]
        public async Task<IActionResult> LoginWithGoogle([FromBody] ExternalAuth user)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.LoginWithGoogle(user);
                return Ok(result);
            }
            return Error(ModelState.Values.First().Errors.First().ErrorMessage);
        }
		[HttpPost]
		public async Task<IActionResult> LoginWithFacebook([FromBody] ExternalAuth user)
		{
			if (ModelState.IsValid)
			{
				var result = await _service.LoginWithFacebook(user);
				return Ok(result);
			}
			return Error(ModelState.Values.First().Errors.First().ErrorMessage);
		}
	}
}
