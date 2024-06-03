
using HMZ.API.Controllers.Base;
using HMZ.Service.Services.CloudinaryServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HMZ.API.Controllers
{


    [Authorize]
    public class UploadController : BaseController<ICloudinaryService>
    {
        public UploadController(ICloudinaryService service) : base(service)
        {
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var result = await _service.UploadImageAsync(file, "images");
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var result = await _service.UploadFile(file, "files");
            return Ok(result);
        }
    }
}