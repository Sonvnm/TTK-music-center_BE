using HMZ.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HMZ.Service.Services.IBaseService
{
    public class ServiceBase<T>
    {
        protected readonly T _unitOfWork;
        protected readonly IServiceProvider _serviceProvider;
        public ServiceBase(T unitOfWork, IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;
        }

        protected async Task<User> GetUserLoginAsync()
        {
            var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var httpContextAccessor = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            return await userManager.GetUserAsync(httpContextAccessor.HttpContext.User);
        }

    }
}
