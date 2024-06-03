


using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

public interface IDashboardService 
{
    Task<DataResult<ChartView>> GetCourseStatistics();

    Task<DataResult<ChartView>> GetTopCoursesSold();
}