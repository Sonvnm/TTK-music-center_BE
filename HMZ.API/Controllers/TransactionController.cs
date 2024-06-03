using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.API.Controllers.Base;
using HMZ.DTOs.Models;
using HMZ.Service.Services.VNPAYServices;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : BaseController<IVNPAYService>
    {
        public TransactionController(IVNPAYService service) : base(service)
        {
        }

        [HttpPost("create-payment-url")]
        public IActionResult CreatePaymentUrl([FromBody] Order_VNPAY model)
        {
            var result = _service.CreatePaymentUrl(model, HttpContext);
            return Ok(new {
                Success = true,
                Data = result
            });

        }
        [HttpGet("payment-callback")]
        public IActionResult PaymentCallback()
        {
            var response = _service.PaymentExecute(Request.Query);
            return Ok(response);
        }
    }
}