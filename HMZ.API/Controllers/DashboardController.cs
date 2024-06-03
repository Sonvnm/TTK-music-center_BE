
using HMZ.Service.Services.LearningProcessServices;
using HMZ.Service.Services.OrderServices;
using HMZ.Service.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
namespace HMZ.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IUserService   _userService;
        private readonly IOrderService  _orderService;
        private readonly ILearningProcessService _learningProcessService;
        private readonly IDashboardService _dashboardService;
        public DashboardController(IUserService userService, IOrderService orderService, ILearningProcessService learningProcessService,IDashboardService dashboardService)
        {
            _userService = userService;
            _orderService = orderService;
            _learningProcessService = learningProcessService;
            _dashboardService=dashboardService;
        }

        [HttpPost("GetDashboardUsers")]
        public async Task<IActionResult> GetDashboardUsers()
        {
            var data = await _userService.GetDashboardData();
            return Ok(data);
        }

        [HttpPost("GetDashboardOrders")]
        public async Task<IActionResult> GetDashboardOrders()
        {
            var data = await _orderService.GetDashboardData();
            return Ok(data);
        }

        [HttpPost("GetDashboardLearningProcess")]
        public async Task<IActionResult> GetDashboardLearningProcess()
        {
            var data = await _learningProcessService.GetDashboardData();
            return Ok(data);
        }

        [HttpGet("GetCourseStatistics")]

        public async Task<IActionResult> GetCourseStatistics()
        {
            var data = await _dashboardService.GetCourseStatistics();
            return Ok(data);

        }
    }
}