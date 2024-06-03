
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.ScheduleDetailServices
{
    public interface IScheduleDetailService : IBaseService<ScheduleDetailQuery, ScheduleDetailView, ScheduleDetailFilter>
    {

        Task<DataResult<ScheduleDetailView>> GetSchedulesDetailByScheduleId(string scheduleId);
        Task<DataResult<ScheduleDetailView>> GetSchedulesDetailByClassId(Guid classId);
        
    }
}