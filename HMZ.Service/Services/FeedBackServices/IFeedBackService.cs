
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.FeedBackServices
{
    public interface IFeedBackService: IBaseService<FeedBackQuery, FeedBackView, FeedBackFilter>
    {
        Task<DataResult<bool>> Approve(int type,Guid? feedBackId);
    }
}