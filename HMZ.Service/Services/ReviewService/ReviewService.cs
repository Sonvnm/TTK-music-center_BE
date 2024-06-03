using HMZ.Database.Entities;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.SDK.Extensions;
using HMZ.Service.Helpers;
using HMZ.Service.Validator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.Service.Services.ReviewService
{
    public class ReviewService : IReviewService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IServiceProvider serviceProvider, IUnitOfWork unitOfWork) { _serviceProvider = serviceProvider; _unitOfWork = unitOfWork; }
        public async Task<DataResult<bool>> CreateAsync(ReviewQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            // Create entity
            var review = new Review
            {
                Comment = entity.Comment,
                Rating = entity.Rating,
                CourseId = entity.CourseId,
                UserId = entity.UserId,


            };
            await _unitOfWork.GetRepository<Review>().Add(review);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Đã thêm mới thành công";
                return result;
            }
            result.Errors.Add("Lỗi khi thêm mới");
            return result;
        }

        public Task<DataResult<int>> DeleteAsync(string[] id)
        {
            throw new NotImplementedException();
        }

        public Task<DataResult<ReviewView>> GetByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<DataResult<ReviewView>> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<DataResult<ReviewView>> GetPageList(BaseQuery<ReviewFilter> query)
        {
            string filterUserName = query.Entity.UserName;
            string filterCourseName = query.Entity.CourseName;

            query.Entity.UserName = null;
            query.Entity.CourseName = null;

            var reviews = _unitOfWork.GetRepository<Review>().AsQueryable()
                .AsNoTracking()
                .Include(u => u.User).Include(x => x.Course)
                .Where(x =>
              (string.IsNullOrEmpty(filterUserName) || x.User.UserName.Contains(filterUserName)) &&
              (string.IsNullOrEmpty(filterCourseName) || x.Course.Name.Contains(filterCourseName))
               ).ApplyFilter(query).OrderByColumns(query.SortColumns, query.SortOrder);
            if (query.SortColumns.Contains("userName"))
            {
                reviews = (query.SortOrder == true ? reviews.OrderBy(x => x.User.UserName) : reviews.OrderByDescending(x => x.User.UserName));
            }
            if (query.SortColumns.Contains("courseName"))
            {
                reviews = (query.SortOrder == true ? reviews.OrderBy(x => x.Course.Name) : reviews.OrderByDescending(x => x.Course.Name));
            }


            var response = new DataResult<ReviewView>();
            response.TotalRecords =await reviews.CountAsync();
            response.Items = reviews.Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                .Take(query.PageSize.Value)
                .Select(x => new ReviewView(x)
                {
                    Id = x.Id,
                    Code = x.Code,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy,
                    IsActive = x.IsActive,

                    Comment = x.Comment,
                    Rating = x.Rating,
                    UserName = x.User.UserName,
                    CourseName = x.Course.Name



                }).ToList();


            return response;
        }

        public async Task<DataResult<Review>> GetReviewsByCourse(Guid courseId)
        {
            var result = new DataResult<Review>();

            var review = await _unitOfWork.GetRepository<Review>().AsQueryable().Include(x => x.User).Where(x => x.CourseId == courseId).ToListAsync();

            if (review.Any())
            {
                result.TotalRecords = review.Count();
                result.Items = review;
            }


            return result;
        }

        public Task<DataResult<int>> UpdateAsync(ReviewQuery entity, string id)
        {
            throw new NotImplementedException();
        }
    }
}
