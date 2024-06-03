using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.HistorySystemServices
{
    public interface IHistorySystemService: IBaseService<HistorySystemQuery, HistorySystemView, HistorySystemFilter>
    {
        
    }
}