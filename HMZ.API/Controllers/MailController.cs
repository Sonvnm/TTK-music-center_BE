using HMZ.API.Controllers.Base;
using HMZ.DTOs.Queries;
using HMZ.Service.MailServices;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class MailController : BaseController<IMailService>
    {
        public MailController(IMailService service) : base(service)
        {
        }

        [HttpPost]
        public async Task<IActionResult> SendMail([FromForm] MailQuery mailRequest)
        {
            if (mailRequest == null)
            {
                return BadRequest();
            }
            await _service.SendEmailAsync(mailRequest);
            return Success(mailRequest);
        }
    }
}
