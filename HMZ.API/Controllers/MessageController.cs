
using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Services.MessageServices;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{

    public class MessageController : CRUDController<IMessageService, MessageQuery, MessageView, MessageFilter>
    {
        public MessageController(IMessageService service) : base(service)
        {
        }

        // Get by class id
        [HttpGet]
        public async Task<IActionResult> GetByClassId(Guid classId, int page = 1, int pageSize = 20)
        {
            var result = await _service.GetByClassIdAsync(classId, page, pageSize);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }

    }
}