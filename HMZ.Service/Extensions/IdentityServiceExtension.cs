using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HMZ.Database.Entities;
using HMZ.Database.Data;
using HMZ.Database.Enums;
using HMZ.Service.Enums;

namespace HMZ.Service.Extensions
{
    public static class IdentityServiceExtension
    {
        public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddIdentityCore<User>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 8;
                opt.Password.RequiredUniqueChars = 1;
                opt.Password.RequireDigit = true;
                //opt.Lockout.DefaultLockoutTimeSpan= TimeSpan.FromMinutes(5);
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.AllowedForNewUsers = true;
            })
                 .AddRoles<Role>()
                 .AddRoleManager<RoleManager<Role>>()
                 .AddSignInManager<SignInManager<User>>()
                 .AddRoleValidator<RoleValidator<Role>>()
                 .AddEntityFrameworkStores<HMZContext>()
                 .AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);

            services.AddAuthentication(
            options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                /// options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["TokenKey"] ?? "This is the default key")),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                // signalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy(EUserRoles.Admin.ToString(), policy => policy.RequireRole(EUserRoles.Admin.ToString()));
                opt.AddPolicy(EUserRoles.Teacher.ToString(), policy => policy.RequireRole(EUserRoles.Admin.ToString(), EUserRoles.Teacher.ToString()));

            });
            services.Configure<IdentityOptions>(opt =>
            {
                // Default SignIn settings.
                opt.SignIn.RequireConfirmedEmail = false;
                opt.SignIn.RequireConfirmedPhoneNumber = false;
                //opt.Lockout.DefaultLockoutTimeSpan= TimeSpan.FromMinutes(5);
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.AllowedForNewUsers = true;
                opt.User.AllowedUserNameCharacters =
                    ConstantConfig.PasswordChar;
                opt.User.RequireUniqueEmail = true;
            });

            return services;
        }
    }
}
