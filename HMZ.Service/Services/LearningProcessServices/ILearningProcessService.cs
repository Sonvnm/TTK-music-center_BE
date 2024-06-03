
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.LearningProcessServices
{
    public interface ILearningProcessService: IBaseService<LearningProcessQuery, LearningProcessView, LearningProcessFilter>
    {
        Task<DataResult<ChartView>> GetDashboardData();
        Task<DataResult<LearningProcessView>> GetByUsername(string userName);
        Task<DataResult<LearningProcessView>> GetByUser(string userName);
    }
}