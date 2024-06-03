using HMZ.Database.Data;
using HMZ.DTOs.Models;
using HMZ.DTOs.Queries;
using HMZ.Service.Helpers;
using HMZ.Service.MailServices;
using HMZ.Service.Services;
using HMZ.Service.Services.ClassServices;
using HMZ.Service.Services.CloudinaryServices;
using HMZ.Service.Services.CourseServices;
using HMZ.Service.Services.DashboardService;
using HMZ.Service.Services.DocumentServices;
using HMZ.Service.Services.FeedBackServices;
using HMZ.Service.Services.FileServices;
using HMZ.Service.Services.HistorySystemServices;
using HMZ.Service.Services.LearningProcessServices;
using HMZ.Service.Services.MessageServices;
using HMZ.Service.Services.OrderServices;
using HMZ.Service.Services.PermissionServices;
using HMZ.Service.Services.ReviewService;
using HMZ.Service.Services.RoleServices;
using HMZ.Service.Services.RoomServices;
using HMZ.Service.Services.ScheduleDetailServices;
using HMZ.Service.Services.ScheduleServices;
using HMZ.Service.Services.StudentStudyProcessServices;
using HMZ.Service.Services.SubjectServices;
using HMZ.Service.Services.TokenServices;
using HMZ.Service.Services.UserServices;
using HMZ.Service.Services.VNPAYServices;
using HMZ.Service.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMZ.Service.Extensions
{
    public static class HMZExtensionService
    {
        public static IServiceCollection AddHMZServices(this IServiceCollection services, IConfiguration configuration)
        {
            #region  Entity DI
            // Default DI
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            // DI Authen
            services.AddScoped(typeof(ITokenService), typeof(TokenService));
            // Custom DI

            services.AddTransient(typeof(IUserService), typeof(UserService));
            services.AddTransient(typeof(IFileService), typeof(FileService));
            services.AddTransient(typeof(IMailService), typeof(MailService));
            services.AddTransient(typeof(IPermissionService), typeof(PermissionService));
            services.AddTransient(typeof(IRoleService), typeof(RoleService));

            services.AddTransient(typeof(IRoomService), typeof(RoomService));
            services.AddTransient(typeof(ISubjectService), typeof(SubjectService));
            services.AddTransient(typeof(ICourseService), typeof(CourseService));
            services.AddTransient(typeof(IClassService), typeof(ClassService));
            services.AddTransient(typeof(IHistorySystemService), typeof(HistorySystemService));
            services.AddTransient(typeof(IFeedBackService), typeof(FeedBackService));
            services.AddTransient(typeof(ICloudinaryService), typeof(CloudinaryService));
            services.AddTransient(typeof(IDocumentService), typeof(DocumentService));
            services.AddTransient(typeof(IScheduleService), typeof(ScheduleService));
            services.AddTransient(typeof(ILearningProcessService), typeof(LearningProcessService));
            services.AddTransient(typeof(IStudentStudyProcessService), typeof(StudentStudyProcessService));
            services.AddTransient(typeof(IOrderService), typeof(OrderService));
            services.AddTransient(typeof(IOrderDetailService), typeof(OrderDetailService));
            services.AddTransient(typeof(IVNPAYService), typeof(VNPAYService));
            services.AddTransient(typeof(IReviewService), typeof(ReviewService));
            services.AddTransient(typeof(IMessageService), typeof(MessageService));
            services.AddTransient(typeof(IScheduleDetailService), typeof(ScheduleDetailService));
            services.AddTransient(typeof(IDashboardService), typeof(DashboardService));
            #endregion

            #region Microservice DI
            #endregion

            #region Validate Extension DI

            services.AddTransient(typeof(IValidator<RegisterQuery>), typeof(RegisterValidator));
            services.AddTransient(typeof(IValidator<UpdatePasswordQuery>), typeof(UpdatePasswordValidator));
            services.AddTransient(typeof(IValidator<UpdatePasswordQuery>), typeof(ResetPasswordValidator));
            services.AddTransient(typeof(IValidator<LoginQuery>), typeof(LoginValidator));
            services.AddTransient(typeof(IValidator<PermissionQuery>), typeof(PermissionValidator));
            services.AddTransient(typeof(IValidator<RoleQuery>), typeof(RoleValidator));

            services.AddTransient(typeof(IValidator<RoomQuery>), typeof(RoomValidator));
            services.AddTransient(typeof(IValidator<SubjectQuery>), typeof(SubjectValidator));
            services.AddTransient(typeof(IValidator<CourseQuery>), typeof(CourseValidator));
            services.AddTransient(typeof(IValidator<ClassQuery>), typeof(ClassValidator));
            services.AddTransient(typeof(IValidator<HistorySystemQuery>), typeof(HistorySystemValidator));
            services.AddTransient(typeof(IValidator<FeedBackQuery>), typeof(FeedBackValidator));
            services.AddTransient(typeof(IValidator<DocumentQuery>), typeof(DocumentValidator));
            services.AddTransient(typeof(IValidator<ScheduleQuery>), typeof(ScheduleValidator));
            services.AddTransient(typeof(IValidator<LearningProcessQuery>), typeof(LearningProcessValidator));
            services.AddTransient(typeof(IValidator<StudentStudyProcessQuery>), typeof(StudentStudyProcessValidator));
            services.AddTransient(typeof(IValidator<OrderQuery>), typeof(OrderValidator));
            services.AddTransient(typeof(IValidator<OrderDetailQuery>), typeof(OrderDetailValidator));
            services.AddTransient(typeof(IValidator<MessageQuery>), typeof(MessageValidator));
            services.AddTransient(typeof(IValidator<ScheduleDetailQuery>), typeof(ScheduleDetailValidator));


            #endregion

            #region Extension DI
            // Add ConnectionString
            services.AddDbContext<HMZContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")
            ));
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.Configure<CloudinarySetting>(configuration.GetSection("CloudinarySetting"));
            services.Configure<VNPAYConfig>(configuration.GetSection("VNPAYConfig"));
            #endregion
            return services;
        }
    }
}
