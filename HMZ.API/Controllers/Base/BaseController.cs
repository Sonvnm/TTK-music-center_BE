
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers.Base
{
    [Route("api/v1/[controller]/[action]")]
    [ApiController]
    public abstract class BaseController<T> : ControllerBase
    {
        protected readonly T _service;
        public BaseController(T service)
        {
            _service = service;
        }

        protected virtual IActionResult Success(object data, string? message = null)
        {
            return Ok(new
            {
                Success = true,
                Items = data,
                Message = message ?? "Success",
            });
        }
        // response error
        protected virtual IActionResult Error(string message)
        {
            return Ok(new
            {
                Success = false,
                Message = new string[] { message },
            });
        }
    }
}
