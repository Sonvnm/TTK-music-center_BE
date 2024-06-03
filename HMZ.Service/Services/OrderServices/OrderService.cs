
using System.ComponentModel.DataAnnotations;
using HMZ.Database.Commons;
using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Extensions;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;
using HMZ.Service.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HMZ.SDK.Extensions;
using HMZ.Service.MailServices;
using HMZ.Service.Services.HistorySystemServices;

namespace HMZ.Service.Services.OrderServices
{
    public class OrderService : ServiceBase<IUnitOfWork>, IOrderService
    {
        private readonly IMailService _mailService;
        private readonly IHistorySystemService _historySystemService;
        public OrderService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider, IMailService mailService, IHistorySystemService historySystemService) : base(unitOfWork, serviceProvider)
        {
            _mailService = mailService;
            _historySystemService = historySystemService;
        }

        public async Task<DataResult<bool>> CheckCourseIsOrdered(Guid courseId)
        {
            var result = new DataResult<bool>();
            var user = await GetUserLoginAsync();
            if (user == null)
            {
                result.Errors.Add("Vui lòng đăng nhập trước khi mua hàng");
                return result;
            }
            var userRole = await _unitOfWork.GetRepository<UserRole>().AsQueryable().Include(x => x.Role).FirstOrDefaultAsync(x => x.UserId == user.Id);
            // check admin and teacher role
            if (userRole.Role.Name == EUserRoles.Admin.ToString() || userRole.Role.Name == EUserRoles.Teacher.ToString())
            {
                result.Errors.Add("Admin và giáo viên không thể mua khóa học");
                return result;
            }
            var userBuyCourse = await _unitOfWork.GetRepository<OrderDetail>().AsQueryable()
                .Include(x => x.Order).ThenInclude(x => x.User)
                .Include(x => x.Course)
                .Where(x => courseId == x.CourseId
                    && x.Order.User.Id == user.Id
                    && x.Order.Status == EOrderStatus.Done
                ).ToListAsync();
            if (userBuyCourse.Any())
            {
                result.Errors.Add("Bạn đã mua khóa học này rồi");
                return result;
            }
            result.Entity = true;
            return result;
        }

        public async Task<DataResult<bool>> CreateAsync(OrderQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<OrderQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Create entity
            var user = await GetUserLoginAsync();
            if (user == null)
            {
                result.Errors.Add("Vui lòng đăng nhập trước khi mua hàng");
                return result;
            }

            var dateNow = DateTime.Now;
            var coursQuery = _unitOfWork.GetRepository<Course>().AsQueryable();

            var userBuyCourse = await _unitOfWork.GetRepository<OrderDetail>().AsQueryable()
                .Include(x => x.Order).ThenInclude(x => x.User)
                .Include(x => x.Course)
                .Where(x => entity.OrderDetails.Select(x => x.CourseId.ToString()).Contains(x.CourseId.ToString())
                    && x.Order.User.Id == user.Id
                    && x.Order.Status == EOrderStatus.Done
                ).ToListAsync();
            if (userBuyCourse.Any())
            {
                result.Errors.Add("Bạn đã mua khóa học này rồi");
                return result;
            }

            var courses = await coursQuery
               .Where(x => entity.OrderDetails.Select(x => x.CourseId.ToString()).Contains(x.Id.ToString())
                   && x.IsActive == true && x.Status == true
                   && x.StartDate >= dateNow
               )
               .ToListAsync();
            if (!courses.Any())
            {
                result.Errors.Add("Khóa học đã ngừng hoặc không tồn tại");
                return result;
            }
            var totalPrice = courses.Sum(x => x.Price);

            var orderExist = await _unitOfWork.GetRepository<Order>().AsQueryable().Where(x => x.Name.Equals(entity.Name)).FirstOrDefaultAsync();

            if (orderExist != null) return result;


            var order = new Order
            {
                Name = entity.Name,
                Description = entity.Description,
                UserId = user.Id,
                TotalPrice = totalPrice,
                Code = entity.TransactionNo,
                Status = entity.Status == "Canceled" ? EOrderStatus.Canceled : EOrderStatus.Done,
                CreatedBy = "System",
            };
            await _unitOfWork.GetRepository<Order>().Add(order);
            // add order detail
            var orderDetails = entity.OrderDetails.Select(x => new OrderDetail
            {
                CourseId = Guid.Parse(x.CourseId),
                OrderId = order.Id,
                CreatedBy = user.UserName,
            }).ToList();
            await _unitOfWork.GetRepository<OrderDetail>().AddRange(orderDetails);

            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                if (order.Status == EOrderStatus.Done)
                {
                    result.Message = "Thêm đơn hàng thành công";
                    // get current dictionary Resource/EmailTemplates 
                    string templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "EmailTemplates");
                    var emailTemplate = File.ReadAllText(Path.Combine(templatesPath, "OrderTemplate.html"));
                    emailTemplate = emailTemplate.Replace("{{OrderCode}}", order.Code);
                    string tableBodyHtml = "";
                    foreach (var item in courses)
                    {
                        tableBodyHtml += $"<tr>"
                                    + $"<td>{item.Name}</td>"
                                    + $"<td>{1}</td>"
                                    + $"<td>{item.Price}</td>"
                                + "</tr>";
                    }
                    tableBodyHtml += $"<tr>"
                                    + $"<td colspan='2'>Tổng cộng</td>"
                                    + $"<td>{totalPrice}</td>"
                                + "</tr>";
                    emailTemplate = emailTemplate.Replace("{{TableBody}}", tableBodyHtml);

                    // Send mail
                    var mail = new MailQuery
                    {
                        ToEmails = new List<string> { user.Email },
                        Subject = $"[Order #{order.Code}] Đơn của bạn đã được tạo thành công",
                        Body = emailTemplate,
                    };
                    await _mailService.SendEmailAsync(mail);
                    // Add history
                    var history = new HistorySystemQuery
                    {
                        UserId = user.Id,
                        Action = $"Tạo đơn hàng {order.Code}",
                        Type = ETypeHistory.Order,
                        Username = user.UserName,
                        Description = $"Người dùng {user.UserName} vừa mua {order.Code}",
                    };
                    await _historySystemService.CreateAsync(history);
                }

                await _unitOfWork.SaveChangesAsync();
                return result;
            }
            result.Errors.Add("Thêm đơn hàng thất bại");
            return result;
        }

        public async Task<DataResult<int>> DeleteAsync(string[] id)
        {
            var result = new DataResult<int>();
            if (id == null || id.Length == 0)
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }
            var orderRepository = _unitOfWork.GetRepository<Order>();
            var orders = await orderRepository.AsQueryable()
                .Where(x => id.Contains(x.Id.ToString()) && x.IsActive == true)
                .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                result.Errors.Add("Không tìm thấy đơn hàng");
                return result;
            }
            orderRepository.DeleteRange(orders, true); // Soft delete
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Xóa danh sách đơn hàng thành công";
                return result;
            }
            result.Errors.Add("Xóa danh sách đơn hàng thất bại");
            return result;
        }

        public async Task<DataResult<OrderView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<OrderView>();
            if (string.IsNullOrEmpty(code))
            {
                result.Errors.Add("Code is null or empty");
                return result;
            }

            var order = await _unitOfWork.GetRepository<Order>().AsQueryable()
                .Include(x => x.User)
                .Include(x => x.OrderDetails).ThenInclude(x => x.Course)
                .Where(x => x.Code == code && x.IsActive == true)
                .Select(x => new OrderView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    Status = x.Status.ToString(),
                    TotalPrice = x.TotalPrice,
                    Username = x.User.UserName,
                    OrderDetails = x.OrderDetails
                    .Where(o => o.IsActive == true && o.OrderId == x.Id)
                    .Select(x => new OrderDetailView(x)
                    {
                        CourseId = x.CourseId,
                        CourseName = x.Course.Name,
                        CoursePrice = x.Course.Price,
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (order == null)
            {
                result.Errors.Add("Không tìm thấy đơn hàng");
                return result;
            }
            result.Entity = order;
            return result;
        }

        public async Task<DataResult<OrderView>> GetByIdAsync(string id)
        {
            var result = new DataResult<OrderView>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }

            var order = await _unitOfWork.GetRepository<Order>().AsQueryable()
                .Include(x => x.User)
                .Include(x => x.OrderDetails).ThenInclude(x => x.Course)
                .Where(x => x.Id.ToString() == id && x.IsActive == true)
                .Select(x => new OrderView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    Status = x.Status.ToString(),
                    TotalPrice = x.TotalPrice,
                    Username = x.User.UserName,
                    OrderDetails = x.OrderDetails
                    .Where(o => o.IsActive == true && o.OrderId == x.Id)
                    .Select(x => new OrderDetailView(x)
                    {
                        CourseId = x.CourseId,
                        CourseName = x.Course.Name,
                        CoursePrice = x.Course.Price,
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (order == null)
            {
                result.Errors.Add("Không tìm thấy đơn hàng");
                return result;
            }
            result.Entity = order;
            return result;
        }

        public async Task<DataResult<ChartView>> GetDashboardData()
        {
            var result = new DataResult<ChartView>();
            var chart = new ChartView();
            var labels = new List<string>();
            var values = new List<int>();
            var dateNow = DateTime.Now;
            var startDate = new DateTime(dateNow.Year, dateNow.Month, 1); // Giới hạn từ đầu tháng
            var endDate = startDate.AddMonths(1).AddDays(-1); // Giới hạn đến cuối tháng

            var orderQuery = _unitOfWork.GetRepository<Order>().AsQueryable();
            var orders = await orderQuery
                .Where(x => x.IsActive == true && x.CreatedAt != null && x.CreatedAt <= endDate) // Thêm kiểm tra CreatedAt không lớn hơn endDate
                .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                result.Errors.Add("Không có dữ liệu");
                return result;
            }
            for (var dt = dateNow; dt <= endDate; dt = dt.AddDays(1))
            {
                var price = orderQuery.Where(x => x.CreatedAt != null && x.CreatedAt.Value.Date == dt.Date).Sum(x => x.TotalPrice);
                labels.Add(dt.ToString("dd/MM/yyyy"));
                values.Add((int)price);
            }

            chart.Labels = labels;
            chart.Values = values;
            chart.Total = await _unitOfWork.GetRepository<Order>().AsQueryable()
                .Where(x => x.IsActive == true)
                .CountAsync();
            var totalToday = await _unitOfWork.GetRepository<Order>().AsQueryable()
                .Where(x => x.IsActive == true && x.CreatedAt != null && x.CreatedAt.Value.Date == dateNow.Date) // Thêm kiểm tra CreatedAt không null
                .CountAsync();
            var totalYesterday = await _unitOfWork.GetRepository<Order>().AsQueryable()
                .Where(x => x.IsActive == true && x.CreatedAt != null && x.CreatedAt.Value.Date == dateNow.AddDays(-1).Date) // Thêm kiểm tra CreatedAt không null
                .CountAsync();
            chart.TotalToday = totalToday;
            chart.TotalYesterday = totalYesterday;
            // Percentage yesterday vs today
            chart.Percentage = totalYesterday == 0 ? 0 : (double)(totalToday - totalYesterday) / totalYesterday * 100;

            result.Entity = chart;
            return result;
        }



        public async Task<DataResult<OrderView>> GetOrderByUserId(string userId)
        {
            var result = new DataResult<OrderView>();


            var order = await _unitOfWork.GetRepository<Order>().AsQueryable()
                .Include(x => x.OrderDetails).ThenInclude(x => x.Course).ThenInclude(x => x.SubjectCourses)
                .Where(x => x.UserId.ToString() == userId)
                .Select(x => new OrderView(x)
                {
                    Name = x.Name,
                    Description = x.Description,
                    Status = x.Status.ToString(),
                    TotalPrice = x.TotalPrice,
                    Username = x.User.UserName,
                    CreatedAt = x.CreatedAt,
                    OrderDetails = x.OrderDetails
                    .Where(o => o.IsActive == true && o.OrderId == x.Id)
                    .Select(x => new OrderDetailView(x)
                    {
                        CourseId = x.CourseId,
                        CourseName = x.Course.Name,
                        CoursePrice = x.Course.Price,
                        CourseImage = x.Course.Image,
                        CourseStartDate = x.Course.StartDate,
                        CourseEndDate = x.Course.EndDate,
                        SubjectsLength = x.Course.SubjectCourses.Count(),
                    }).ToList()
                }).ToListAsync();

            if (order == null)
            {
                result.Errors.Add("Không tìm thấy đơn hàng");
                return result;
            }
            result.Items = order;
            return result;
        }

        public async Task<DataResult<OrderView>> GetPageList(BaseQuery<OrderFilter> query)
        {
            var result = new DataResult<OrderView>();

            string usernameFilter = query?.Entity?.Username;
            if (query.Entity != null && !string.IsNullOrEmpty(usernameFilter))
            {
                query.Entity.Username = null;
            }



            var orderQuery = _unitOfWork.GetRepository<Order>().AsQueryable()
            .Include(x => x.User)
            .Include(x => x.OrderDetails)
            .ThenInclude(od => od.Course)
            .Where(x => x.IsActive == true)
            .ApplyFilter(query);

            if (!string.IsNullOrEmpty(usernameFilter))
            {
                orderQuery = orderQuery.Where(x => x.User.UserName.Contains(usernameFilter));
            }

            var orderedOrders = orderQuery
                .OrderBy(x => x.OrderDetails.FirstOrDefault().Course.Name)
                .ToList();


            result.TotalRecords = orderedOrders.Count();
            result.Items = orderedOrders
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new OrderView(x)
                    {
                        Name = x.Name,
                        Description = x.Description,
                        Status = x.Status.ToString(),
                        TotalPrice = x.TotalPrice,
                        Username = x.User.UserName,
                        CourseName = x.OrderDetails.FirstOrDefault().Course.Name,

                    }).ToList();
            return result;
        }

        public Task<DataResult<int>> UpdateAsync(OrderQuery entity, string id)
        {
            throw new NotImplementedException();
        }

        public async Task<DataResult<bool>> UpdateStatusAsync(Guid id, EOrderStatus status)
        {
            var result = new DataResult<bool>();
            var order = await _unitOfWork.GetRepository<Order>().AsQueryable()
                .Where(x => x.Id == id && x.IsActive == true)
                .FirstOrDefaultAsync();
            if (order == null)
            {
                result.Errors.Add("Không tìm thấy đơn hàng");
                return result;
            }
            var user = await GetUserLoginAsync();
            order.Status = status;
            order.UpdatedBy = user.UserName;
            order.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Order>().Update(order);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                result.Message = "Cập nhật trạng thái đơn hàng thành công";
                return result;
            }
            result.Errors.Add("Cập nhật trạng thái đơn hàng thất bại");
            return result;
        }
    }
}