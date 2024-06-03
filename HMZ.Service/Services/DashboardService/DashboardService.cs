using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.Service.Services.DashboardService
{
    public class DashboardService : IDashboardService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public DashboardService(UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;

        }
        public async Task<DataResult<ChartView>> GetTopCoursesSold()
        {
            var result = new DataResult<ChartView>();

            var courses = await _unitOfWork.GetRepository<Course>().AsQueryable().ToListAsync();
            var orders = await _unitOfWork.GetRepository<Order>().AsQueryable().Include(x => x.OrderDetails).ThenInclude(x => x.Course).Where(x => x.Status.Equals(EOrderStatus.Done)).GroupBy(x => x.OrderDetails.FirstOrDefault().CourseId).ToListAsync();



            result.Entity = new ChartView()
            {
                NewCourse = courses.Count(x => DateTime.Compare(x.StartDate.Value, DateTime.Now.Date) > 0),
                ProcessingCourse = courses.Count(x => DateTime.Compare(x.StartDate.Value.Date, DateTime.Now.Date) <= 0 && DateTime.Compare(DateTime.Now, x.EndDate.Value.Date) <= 0),
                EndCourse = courses.Count(x => DateTime.Compare(DateTime.Now, x.EndDate.Value.Date) >= 0),
            };

            return result;
        }
        public async Task<DataResult<ChartView>> GetCourseStatistics()
        {
            var result = new DataResult<ChartView>();

            var courses = await _unitOfWork.GetRepository<Course>().AsQueryable().ToListAsync();
            var orders = await _unitOfWork.GetRepository<Order>().AsQueryable().Include(x => x.OrderDetails).ThenInclude(x => x.Course).ToListAsync();
            var topCourses = new List<TopCourse>();
            var ordersCourse = orders.Where(x => x.Status.Equals(EOrderStatus.Done)).GroupBy(x => x.OrderDetails.FirstOrDefault().CourseId).ToList();
            if (ordersCourse.Any())
            {
                foreach (var order in ordersCourse)
                {
                    var totalPrice = order.Select(x => x.TotalPrice).Sum();
                    topCourses.Add((new TopCourse { CourseName = order.FirstOrDefault().OrderDetails[0].Course.Name, TotalOrder = order.Count(), TotalPrice = totalPrice }));
                }
            }

            var failOrder = orders.Where(x => x.Status.Equals(EOrderStatus.Canceled)).Count();
            var pendingOrder = orders.Where(x => x.Status.Equals(EOrderStatus.Pending)).Count();
            var doneOrder = orders.Where(x => x.Status.Equals(EOrderStatus.Done)).Count();


            result.Entity = new ChartView()
            {
                TopCourse = topCourses,
                OrderStatistic = new OrderStatistic { DoneOrder = doneOrder, FailOrder = failOrder, PendingOrder = pendingOrder },
                NewCourse = courses.Count(x => DateTime.Compare(x.StartDate.Value, DateTime.Now.Date) > 0),
                ProcessingCourse = courses.Count(x => DateTime.Compare(x.StartDate.Value.Date, DateTime.Now.Date) <= 0 && DateTime.Compare(DateTime.Now, x.EndDate.Value.Date) <= 0),
                EndCourse = courses.Count(x => DateTime.Compare(DateTime.Now, x.EndDate.Value.Date) >= 0),
            };

            return result;
        }

        public async Task<DataResult<ChartView>> GetDashboardDatas()
        {
            var response = new DataResult<ChartView>();

            var listValue = new List<int>();
            List<string> months = new List<string>();
            var users = await _userManager.Users.ToListAsync();
            for (var i = 1; i < 13; i++)
            {
                months.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i));
                var count = users.Where(x => x.CreatedAt != null && x.CreatedAt.Value.Month == i).Count(); // Added null check for CreatedAt
                listValue.Add(count);
            }
            /* var listDates = new List<DateTime>();
             for (var dt = startDate; dt <= endDate; dt = dt.AddDays(1))
             {
                 listDates.Add(dt);
             }*/



            response.Entity = new ChartView()
            {
                /*Labels = listDates.Select(d => d.ToString("dd/MM/yyyy")).ToList(), */
                Labels = months.ToList(),
                Values = listValue,
                Total = users.Count,
                /*TotalToday = users.Where(x => x.CreatedAt != null && x.CreatedAt.Value.Date == dateNow.Date).Count(), // Added null check for CreatedAt
                TotalYesterday = users.Where(x => x.CreatedAt != null && x.CreatedAt.Value.Date == dateNow.AddDays(-1).Date).Count(), // Added null check for CreatedAt
                // yesterday vs today
                Percentage = users.Count > 0 ? (users.Where(x => x.CreatedAt != null && x.CreatedAt.Value.Date == dateNow.Date).Count() - users.Where(x => x.CreatedAt != null && x.CreatedAt.Value.Date == dateNow.AddDays(-1).Date).Count()) / users.Count : 0 // Added null check for CreatedAt*/
            };
            return response;
        }


    }
}
