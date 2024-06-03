using HMZ.Database.Entities;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.Service.Services.ReviewService
{
    public interface IReviewService:IBaseService<ReviewQuery,ReviewView,ReviewFilter>
    {

        Task<DataResult<Review>> GetReviewsByCourse(Guid courseId);
    }
}
