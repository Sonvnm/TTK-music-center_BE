using HMZ.Database.Entities;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.ScheduleServices
{
    public interface IScheduleService : IBaseService<ScheduleQuery, ScheduleView, ScheduleFilter>
    {
         Task<DataResult<ScheduleView>> GetSchedulesByUser(string userName);
         Task<DataResult<bool>> SendMail(string scheduleId,Guid scheduleDetailId, Guid? roomId);
    }
}
