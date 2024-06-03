
using HMZ.API.Controllers.Base;
using HMZ.DTOs.Queries;
using HMZ.Service.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HMZ.API.Controllers
{
    public class PaymentSalaryController : BaseController<IUserService>
    {
        public PaymentSalaryController(IUserService service) : base(service)
        {
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CalculateSalaryForTeacher([FromBody] CalculateSalaryQuery query)
        {
            var result = await _service.CalculateSalaryForTeacher(query);
            return Ok(result);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PaymentSalaryForTeacher([FromForm] CalculateSalaryQuery query)
        {
            var result = await _service.PaymentSalaryForTeacher(query);
            return Ok(result);
        }
    }
}