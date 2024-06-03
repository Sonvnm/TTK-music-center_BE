using System.ComponentModel.DataAnnotations;
using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.SDK.Extensions;
using HMZ.Service.Extensions;
using HMZ.Service.Helpers;
using HMZ.Service.MailServices;
using HMZ.Service.Services.IBaseService;
using HMZ.Service.Services.HistorySystemServices;
using HMZ.Service.Services.TokenServices;
using HMZ.Service.Validator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HMZ.Service.Services.CloudinaryServices;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace HMZ.Service.Services.UserServices
{
    public class UserService : ServiceBase<IUnitOfWork>, IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMailService _mailService;
        private readonly IHistorySystemService _historySystemService;
        private readonly ICloudinaryService _cloudinaryService;
        public UserService(IUnitOfWork unitOfWork, UserManager<User> userManager,
        IHistorySystemService historySystemService
        , IServiceProvider serviceProvider, SignInManager<User> signInManager, ITokenService tokenService, IMailService mailService,
        ICloudinaryService cloudinaryService

        ) : base(unitOfWork, serviceProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mailService = mailService;
            _historySystemService = historySystemService;
            _cloudinaryService = cloudinaryService;
        }
        public async Task<DataResult<bool>> CreateAsync(UserQuery entity)
        {
            var response = new DataResult<bool>();
            if (entity == null)
            {
                response.Errors.Add("User is null");
                return response;
            }
            var user = new User()
            {
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                UserName = entity.Email.Split("@")[0].ToLower(),
                DateOfBirth = entity.DateOfBirth.Value,
                IsActive = entity.IsActive ?? true,
                Image = entity.Image,
                PublicId = entity.PublicId,

            };
            var result = await _userManager.CreateAsync(user, entity.Password);
            if (!result.Succeeded)
            {
                response.Errors.Add(result.Errors.Select(x => x.Description).FirstOrDefault());
                return response;
            }
            entity.Roles = entity.Roles != null && entity.Roles.Length > 0 ? entity.Roles : new string[] { EUserRoles.Member.ToString() };
            var roleResult = await _userManager.AddToRolesAsync(user, entity.Roles);
            if (!roleResult.Succeeded)
            {
                response.Errors.Add(roleResult.Errors.Select(x => x.Description).FirstOrDefault());
                return response;
            }
            response.Entity = true;
            response.Message = "Thêm mới thành công";
            return response;
        }
        public async Task<DataResult<int>> DeleteAsync(string[] id)
        {
            var response = new DataResult<int>();
            if (id == null || id.Length == 0)
            {
                response.Errors.Add("Id is null");
                return response;
            }
            var users = await _userManager.Users.Where(x => id.Contains(x.Id.ToString().ToLower())).ToListAsync();
            if (users == null || users.Count == 0)
            {
                response.Errors.Add("User not found");
                return response;
            }

            var studentInClass = await _unitOfWork.GetRepository<StudentClass>().AsQueryable().Include(x => x.User).Where(x => id.Contains(x.UserId.ToString())).GroupBy(x => x.UserId).ToListAsync();

            if (studentInClass.Any())
            {
                response.Errors.Add($"{string.Join(",", studentInClass.Select(x => x.Select(y => y.User.UserName).FirstOrDefault()))} đang tồn tại trong lớp học không thể xóa !");
                return response;
            }
            // check  user is loginning
            foreach (var user in users)
            {
                if (user.UserName == _userManager.GetUserName(_serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>().HttpContext.User))
                {
                    response.Errors.Add("User is loginning");
                    return response;
                }
            }



            // delete range
            foreach (var user in users)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    response.Errors.Add(result.Errors.Select(x => x.Description).FirstOrDefault());
                    response.Entity += 1;
                    continue;
                }
            }
            response.Message = "Xóa thành công";
            return response;
        }
        public async Task<DataResult<UserView>> GetByIdAsync(string id)
        {
            var response = new DataResult<UserView>();
            if (string.IsNullOrEmpty(id))
            {
                response.Errors.Add("Id is null");
                return response;
            }
            var user = await _userManager.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.Id.ToString().ToLower() == id.ToLower());
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }
            response.Entity = new UserView()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Username = user.UserName,
                Image = user.Image,
                AccountType = user.AccountType,
                Roles = user.UserRoles.Select(x => x.Role.Name).ToList(),
                // default  properties
                Code = user.Code,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                CreatedBy = user.CreatedBy,
                IsActive = user.IsActive
            };
            return response;
        }
        public async Task<DataResult<UserView>> GetPageList(BaseQuery<UserFilter> query)
        {
            string roleFilter = null;
            if (query.Entity != null && !string.IsNullOrEmpty(query.Entity.Roles))
            {
                roleFilter = query.Entity.Roles;
                query.Entity.Roles = null;
            }
            // remove role property in query
            var userQuery = _userManager.Users
                .AsNoTracking()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Where(x => string.IsNullOrEmpty(roleFilter) || x.UserRoles.Any(ur => roleFilter.Contains(ur.Role.Name)))
                .ApplyFilter(query)
                .OrderByColumns(query.SortColumns, query.SortOrder);

            var response = new DataResult<UserView>();
            response.TotalRecords = await userQuery.CountAsync();
            response.Items = await userQuery.Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                .Take(query.PageSize.Value)
                .Select(x => new UserView()
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    DateOfBirth = x.DateOfBirth,
                    Username = x.UserName,
                    Image = x.Image,
                    AccountType = x.AccountType,
                    PhoneNumber = x.PhoneNumber,
                    Roles = x.UserRoles.Select(x => x.Role.Name).ToList(),
                    // default properties
                    Code = x.Code,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    CreatedBy = x.CreatedBy,
                    UpdatedBy = x.UpdatedBy,
                    IsActive = x.IsActive
                })
            .ToListAsync();

            return response;
        }

        public async Task<DataResult<int>> UpdateAsync(UserQuery entity, string id)
        {
            var response = new DataResult<int>();
            if (entity == null)
            {
                response.Errors.Add("Entity is null");
                return response;
            }
            if (string.IsNullOrEmpty(id))
            {
                response.Errors.Add("Id is null");
                return response;
            }
            // validate IsPhone Number
            // length 10
            var regex = new Regex(@"^0\d{9}$");
            if (!string.IsNullOrEmpty(entity.PhoneNumber) && !regex.IsMatch(entity.PhoneNumber))
            {
                response.Errors.Add("Số điện thoại phải là 10 số và bắt đầu bằng số 0");
                return response;
            }


            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }

            if (!string.IsNullOrEmpty(user.Image) && entity.Image != user.Image && !string.IsNullOrEmpty(user.Image))
            {
                string[] paths = user.Image.Split("/");
                var publicId = paths[paths.Length - 2] + "/" + paths[paths.Length - 1].Split(".")[0];
                var checkResult = await _cloudinaryService.DeleteImageAsync(publicId);
                if (checkResult.Result != null)
                {
                    user.Image = null;
                }
            }

            user.Email = entity.Email ?? user.Email;
            user.UserName = user.Email.Split("@")[0].ToLower();
            user.FirstName = entity.FirstName ?? user.FirstName;
            user.LastName = entity.LastName ?? user.LastName;
            user.DateOfBirth = entity.DateOfBirth.Value != null ? entity.DateOfBirth.Value : user.DateOfBirth;
            user.IsActive = entity.IsActive ?? user.IsActive;
            user.Image = entity.Image ?? user.Image;
            user.PublicId = entity.PublicId ?? user.PublicId;
            user.PhoneNumber = entity.PhoneNumber ?? user.PhoneNumber;

            user.UpdatedAt = DateTime.Now;
            user.UpdatedBy = entity.UpdatedBy ?? user.UpdatedBy;

            if (!string.IsNullOrEmpty(entity.Password))
            {
                // change password not old password
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, entity.Password);
            }
            // check role change
            var roles = _userManager.GetRolesAsync(user).Result.ToList();
            if (entity.Roles != null && entity.Roles.Length > 0)
            {
                var role = await _userManager.RemoveFromRolesAsync(user, roles);
                if (!role.Succeeded)
                {
                    response.Errors.Add(role.Errors.Select(x => x.Description).FirstOrDefault());
                    return response;
                }
                role = await _userManager.AddToRolesAsync(user, entity.Roles);
                if (!role.Succeeded)
                {
                    response.Errors.Add(role.Errors.Select(x => x.Description).FirstOrDefault());
                    return response;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                response.Errors.Add(result.Errors.Select(x => x.Description).FirstOrDefault());
                return response;
            }
            response.Entity = 1;
            return response;
        }
        private async Task<DataResult<UserLoginView>> CreateToken(User user)
        {
            var accessToken = await _tokenService.CreateToken(user);
            var refreshToken = await _tokenService.GenerateRefreshToken();
            var response = new DataResult<UserLoginView>();
            response.Entity = new UserLoginView()
            {
                Email = user.Email,
                Username = user.UserName,
                Token = new TokenView()
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiryTime = DateTime.Now.AddDays(7)
                }
            };
            return response;
        }
        public async Task<DataResult<UserLoginView>> Login(LoginQuery entity)
        {
            // Log history
            var history = new HistorySystemQuery()
            {
                Action = "Login",
                Description = $"{entity.Email} login at {DateTime.Now}",
                Type = ETypeHistory.Login,
                Username = entity.Email
            };
            await _historySystemService.CreateAsync(history);
            await _unitOfWork.SaveChangesAsync();

            var response = new DataResult<UserLoginView>();
            // Validate
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<LoginQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                response.Errors.AddRange(resultValidator.JoinError());
                return response;
            }

            // check email or username
            if (entity.Email.Contains("@"))
            {
                string username = entity.Email.Split("@")[0].ToLower();
                var user = await _userManager.FindByNameAsync(username);
                var result = await _signInManager.CheckPasswordSignInAsync(user, entity.Password, entity.RememberMe.Value);
                if (result.IsLockedOut)
                {
                    response.Errors.Add("Account is locked");
                    return response;
                }
                if (!result.Succeeded)
                {
                    response.Errors.Add("Password is incorrect! Wrong input more than 5 times will be locked");
                    return response;
                }
                response = await CreateToken(user);
                return response;

            }
            else
            {
                var user = await _userManager.FindByNameAsync(entity.Email);
                var result = await _signInManager.CheckPasswordSignInAsync(user, entity.Password, entity.RememberMe.Value);
                if (result.IsLockedOut)
                {
                    response.Errors.Add("Account is locked");
                    return response;
                }
                if (!result.Succeeded)
                {
                    response.Errors.Add("Sai mật khẩu ! Vui lòng thử lại");
                    return response;
                }
                response = await CreateToken(user);
                return response;
            }
        }
        public async Task<DataResult<UserLoginView>> Register(RegisterQuery entity)
        {
            // Log history
            var history = new HistorySystemQuery()
            {
                Action = "Register",
                Description = $"{entity.Email} register at {DateTime.Now}",
                Type = ETypeHistory.Create,
                Username = entity.Email
            };
            await _historySystemService.CreateAsync(history);
            await _unitOfWork.SaveChangesAsync();

            var response = new DataResult<UserLoginView>();
            // Validate
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<RegisterQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                response.Errors.AddRange(resultValidator.JoinError());
                return response;
            }
            var user = new User()
            {
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                DateOfBirth = entity.DateOfBirth.HasValue ? entity.DateOfBirth.Value : DateTime.Now,
                UserName = entity.Email.Split("@")[0].ToLower(),
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, entity.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, EUserRoles.Member.ToString());
            }
            if (result.Errors.Any())
            {
                response.Errors.AddRange(result.Errors.Select(x => x.Description));
                return response;
            }
            response = await CreateToken(user);
            return response;
        }
        public async Task<DataResult<bool>> ChangeRole(ChangeRoleQuery entity)
        {
            var response = new DataResult<bool>();
            if (entity == null)
            {
                response.Errors.Add("Entity is null");
                return response;
            }
            var user = await _userManager.FindByIdAsync(entity.UserId);
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }
            var roles = _userManager.GetRolesAsync(user).Result.ToList();
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                response.Errors.Add(result.Errors.Select(x => x.Description).FirstOrDefault());
                return response;
            }
            result = await _userManager.AddToRolesAsync(user, entity.Roles);
            if (!result.Succeeded)
            {
                response.Errors.Add(result.Errors.Select(x => x.Description).FirstOrDefault());
                return response;
            }
            response.Entity = true;
            return response;
        }
        public async Task<DataResult<UserView>> GetByUserName(string userName)
        {
            var response = new DataResult<UserView>();
            if (string.IsNullOrEmpty(userName))
            {
                response.Errors.Add("UserName is null");
                return response;
            }
            var user = await _userManager.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }
            response.Entity = new UserView()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Username = user.UserName,
                Image = user.Image,
                PhoneNumber = user.PhoneNumber,
                AccountType = user.AccountType,
                Roles = user.UserRoles.Select(x => x.Role.Name).ToList(),
                // default  properties
                Code = user.Code,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                CreatedBy = user.CreatedBy,
                IsActive = user.IsActive
            };
            return response;
        }
        public async Task<DataResult<bool>> UpdatePassword(UpdatePasswordQuery entity)
        {
            var response = new DataResult<bool>();

            // Validate
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<UpdatePasswordQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                response.Errors.AddRange(resultValidator.JoinError());
                return response;
            }

            var user = await _userManager.FindByEmailAsync(entity.Email);
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }
            var result = await _userManager.ChangePasswordAsync(user, entity.OldPassword, entity.NewPassword);
            if (!result.Succeeded)
            {
                response.Errors.Add(result.Errors.Select(x => x.Description).FirstOrDefault());
                return response;
            }
            response.Entity = true;
            return response;
        }
        public async Task<DataResult<bool>> UploadAvatar(UploadAvatarQuery entity)
        {
            var response = new DataResult<bool>();
            if (entity == null)
            {
                response.Errors.Add("Entity is null");
                return response;
            }
            var user = await _userManager.FindByIdAsync(entity.UserId);
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }
            user.Image = entity.ImagePath;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                response.Errors.Add(result.Errors.Select(x => x.Description).FirstOrDefault());
                return response;
            }
            response.Entity = true;
            return response;
        }

        public async Task<DataResult<bool>> ForgotPassword(string email, string host)
        {
            var response = new DataResult<bool>();
            if (string.IsNullOrEmpty(email))
            {
                response.Errors.Add("Email is null");
                return response;
            }
            string username = email.Split("@")[0].ToLower();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _unitOfWork.SaveChangesAsync();
            // get localhost url
            string url = $"{host}?token={token}&email={email}";
            MailQuery mailQuery = new MailQuery()
            {
                ToEmails = new List<string>() { email },
                Subject = "Reset Password",
                Body = $"<h1>Reset Password</h1> <p>To reset your password, click here: <a href='{url}'>Reset Password</a></p>"
            };
            await _mailService.SendEmailAsync(mailQuery);
            response.Entity = true;
            return response;

        }

        public async Task<DataResult<bool>> ResetPassword(UpdatePasswordQuery entity)
        {
            var response = new DataResult<bool>();
            // Validate
            await using var scope = _serviceProvider.CreateAsyncScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<UpdatePasswordQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                response.Errors.AddRange(resultValidator.JoinError());
                return response;
            }
            string username = entity.Email.Split("@")[0].ToLower();
            var user = await _userManager.FindByNameAsync(username);
            entity.Token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, entity.Token, entity.NewPassword);
            if (!result.Succeeded)
            {
                response.Errors.Add(result.Errors.Select(x => x.Description).FirstOrDefault());
                return response;
            }
            response.Entity = true;
            return response;
        }

        public async Task<DataResult<bool>> LockUser(string username, bool isLock)
        {
            var response = new DataResult<bool>();
            if (string.IsNullOrEmpty(username))
            {
                response.Errors.Add("UserName is null");
                return response;
            }
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }
            user.LockoutEnabled = isLock;
            user.LockoutEnd = isLock ? DateTime.Now.AddYears(100) : null;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                response.Errors.Add(result.Errors.Select(x => x.Description).FirstOrDefault());
                return response;
            }
            response.Entity = true;
            return response;
        }

        public async Task<DataResult<UserView>> GetByCodeAsync(string code)
        {
            var response = new DataResult<UserView>();
            if (string.IsNullOrEmpty(code))
            {
                response.Errors.Add("Code is null");
                return response;
            }
            var user = await _unitOfWork.GetRepository<User>().AsQueryable().FirstOrDefaultAsync(x => x.Code == code);
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }
            response.Entity = new UserView()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Username = user.UserName,
                Image = user.Image,
                AccountType = user.AccountType,
                Roles = _userManager.GetRolesAsync(user).Result.ToList(),
                // default  properties
                Code = user.Code,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                CreatedBy = user.CreatedBy,
                IsActive = user.IsActive
            };
            return response;
        }

        public async Task<DataResult<UserLoginView>> LoginWithGoogle(ExternalAuth entity)
        {
            var response = new DataResult<UserLoginView>();
            var payload = await _tokenService.VerifyGoogleToken(entity);
            if (payload == null)
            {
                response.Errors.Add("Invalid External Authentication.");
                return response;
            }
            var info = new UserLoginInfo(entity.Provider, payload.Subject, entity.Provider);
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new User()
                    {
                        Email = payload.Email,
                        UserName = payload.Email.Split("@")[0].ToLower(),
                        FirstName = payload.GivenName,
                        LastName = payload.FamilyName,
                        Image = payload.Picture,
                        DateOfBirth = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        AccountType = EAccountType.Google,
                        IsActive = true
                    };
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, EUserRoles.Member.ToString());
                    }
                    if (result.Errors.Any())
                    {
                        response.Errors.AddRange(result.Errors.Select(x => x.Description));
                        return response;
                    }
                }
                await _userManager.AddLoginAsync(user, info);
            }
            response = await CreateToken(user);
            return response;
        }

        public async Task<DataResult<UserLoginView>> LoginWithFacebook(ExternalAuth entity)
        {
            var response = new DataResult<UserLoginView>();
            var userView = await _tokenService.VerifyFacebookToken(entity);
            if (userView == null)
            {
                response.Errors.Add("Invalid External Authentication.");
                return response;
            }
            var info = new UserLoginInfo(entity.Provider, userView.Id.ToString(), entity.Provider);
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(userView.Email);
                await _userManager.AddLoginAsync(user, info);
            }
            response = await CreateToken(user);
            return response;
        }

        public async Task<DataResult<bool>> ChangePassword(UpdatePasswordQuery query)
        {
            var response = new DataResult<bool>();
            // Validate
            await using var scope = _serviceProvider.CreateAsyncScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<UpdatePasswordQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(query);
            if (resultValidator.HasError())
            {
                response.Errors.AddRange(resultValidator.JoinError());
                return response;
            }
            var user = await _userManager.FindByEmailAsync(query.Email);
            var result = await _userManager.ChangePasswordAsync(user, query.OldPassword, query.NewPassword);
            if (!result.Succeeded)
            {
                response.Errors.Add(result.Errors.Select(x => x.Description).FirstOrDefault());
                return response;
            }
            response.Message = "Đổi mật khẩu thành công";
            response.Entity = true;
            return response;
        }

        public async Task<DataResult<CalculateSalaryView>> CalculateSalaryForTeacher(CalculateSalaryQuery query)
        {
            var response = new DataResult<CalculateSalaryView>();
            if (query == null)
            {
                response.Errors.Add("Query is null");
                return response;
            }
            var user = await _userManager.FindByNameAsync(query.Username);
            if (user == null)
            {
                response.Errors.Add("Giáo viên không tồn tại");
                return response;
            }
            // calculate salary
            decimal salary = 0;

            var fromDate = query.CalculateDate.FromValue ?? DateTime.Now;
            var toDate = query.CalculateDate.ToValue ?? DateTime.Now;


            var dateNow = DateTime.Now;
            var fistDateMonth = new DateTime(dateNow.Year, dateNow.Month, 1);
            var lastDateMonth = fistDateMonth.AddMonths(1).AddDays(-1);





            var learningProcess = await _unitOfWork.GetRepository<LearningProcess>()
                .AsQueryable()
                .Include(x => x.ScheduleDetail)
                .Where(x => x.ScheduleDetail.StartTime.Value.Date >= fromDate.Date
                    && x.ScheduleDetail.EndTime.Value.Date <= toDate.Date
                    && x.CreatedBy == user.UserName && x.Status.Equals(ELearningProcessStatus.Done) && x.ScheduleDetail.IsMakeUpClass != true
                    && x.IsActive == true)
                .ToListAsync();
            if (learningProcess != null && learningProcess.Count > 0)
            {
                salary = (2 * learningProcess.Count) * 150000;
            }
            response.Entity = new CalculateSalaryView()
            {
                Username = user.UserName,
                Amount = salary,
                CalculateDate = query.CalculateDate ?? null,
                TotalDay = learningProcess.Count,
                TotalTime = learningProcess.Count * 2,
                Description = "Lương tháng " + dateNow.Month + " năm " + dateNow.Year + " của giáo viên " + user.UserName
            };
            return response;

        }

        public async Task<DataResult<bool>> PaymentSalaryForTeacher([FromForm] CalculateSalaryQuery query)
        {
            var response = new DataResult<bool>();
            if (query == null)
            {
                response.Errors.Add("Query is null");
                return response;
            }
            var user = await _userManager.FindByNameAsync(query.Username);
            if (user == null)
            {
                response.Errors.Add("Giáo viên không tồn tại");
                return response;
            }

            // calculate salary
            decimal salary = 0;
            string format = " dd MM yyyy HH:mm:ss";
            DateTime fromDate;
            DateTime.TryParse(query.FromDate, out fromDate);
            DateTime toDate;
            DateTime.TryParse(query.ToDate, out toDate);
            bool isExist = await IsExistPayment(fromDate, user.UserName);
            if (isExist)
            {
                response.Errors.Add("Đã thanh toán lương tháng này");
                return response;
            }

            var dateNow = DateTime.Now;
            var fistDateMonth = new DateTime(dateNow.Year, dateNow.Month, 1);
            var lastDateMonth = fistDateMonth.AddMonths(1).AddDays(-1);


            var learningProcess = await _unitOfWork.GetRepository<LearningProcess>()
                .AsQueryable()
                .Include(x => x.ScheduleDetail)
                .Where(x => x.ScheduleDetail.StartTime.Value.Date >= fromDate.Date
                    && x.ScheduleDetail.EndTime.Value.Date <= toDate.Date
                    && x.CreatedBy == user.UserName && x.Status.Equals(ELearningProcessStatus.Done)
                    && x.IsActive == true && x.ScheduleDetail.IsMakeUpClass!=true)
                .ToListAsync();
            if (learningProcess != null && learningProcess.Count > 0)
            {
                salary = (2 * learningProcess.Count) * 150000;
            }

            if (salary == 0)
            {
                response.Errors.Add("Thanh toán tối thiểu 300.000 đ");
                return response;
            }
            var userLogin = await GetUserLoginAsync();
            if (userLogin == null)
            {
                response.Errors.Add("User login not found");
                return response;
            }

            // payment
            var payment = new HistorySystem()
            {
                UserId = user.Id,
                Description = $"Thanh toán lương tháng {dateNow.Month} năm {dateNow.Year} của giáo viên {user.UserName}",
                Type = ETypeHistory.Transaction,
                Code = "PAY_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + "_" + fromDate.ToString("ddMMyyyy"),
                IsActive = true,
                Price = salary,
                Action = "Trả lương"
            };
            await _unitOfWork.GetRepository<HistorySystem>().Add(payment);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {

                // sen mail
                MailQuery mailQuery = new MailQuery()
                {
                    ToEmails = new List<string>() { user.Email },
                    Subject = "[Paylist] Thanh toán lương tháng " + dateNow.Month + " năm " + dateNow.Year,
                    Body = $"<h1>Thanh toán lương</h1> <p>Bảng tính lương tháng {dateNow.Month} năm {dateNow.Year}</p>",
                    Attachments = new List<IFormFile> { query.File }

                };
                await _mailService.SendEmailAsync(mailQuery);
            }
            response.Message = "Thanh toán lương thành công";
            response.Entity = true;
            return response;
        }

        private async Task<bool> IsExistPayment(DateTime? fromDate, string username)
        {

            // example: fro
            var payment = await _unitOfWork.GetRepository<HistorySystem>()
                .AsQueryable().Include(x => x.User).Where(x => x.CreatedAt.Value.Month == fromDate.Value.Month
                       && x.Type == ETypeHistory.Transaction).ToListAsync();
            var result = payment.Where(x =>
                        x.User.UserName == username
                       && x.IsActive == true)
                   .FirstOrDefault();
            return result != null;
        }

        public async Task<DataResult<ChartView>> GetDashboardData()
        {
            var response = new DataResult<ChartView>();

            var listValue = new List<int>();
            var listSells = new List<decimal>();
            var listRevenues = new List<decimal>();
            var listPaidSalary = new List<decimal>();
            List<string> months = new List<string>();
            var users = await _userManager.Users.ToListAsync();
            var orders = await _unitOfWork.GetRepository<Order>().AsQueryable().Include(x => x.OrderDetails).ThenInclude(x => x.Course).ToListAsync();
            var doneCourses = orders.Where(x => x.Status.Equals(EOrderStatus.Done)).ToList();
            var historiesTransactionSalary = await _unitOfWork.GetRepository<HistorySystem>()
                .AsQueryable().Include(x => x.User).Where(x => x.Type == ETypeHistory.Transaction).ToListAsync();
            for (var i = 1; i < 13; i++)
            {
                months.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i));
                var count = users.Where(x => x.CreatedAt != null && x.CreatedAt.Value.Month == i).Count(); // Added null check for CreatedAt
                var countSell = doneCourses.Where(x => x.CreatedAt != null && x.CreatedAt.Value.Month == i).Select(x => x.TotalPrice).Sum();
                var totalPaidSalaryByMonth = (decimal)historiesTransactionSalary.Where(x => x.CreatedAt != null && x.CreatedAt.Value.Month == i).Select(x => x.Price).Sum();

                listPaidSalary.Add(totalPaidSalaryByMonth);
                listValue.Add(count);
                listSells.Add(countSell);
                var revenue = countSell - totalPaidSalaryByMonth;
                listRevenues.Add(revenue);
            }


            response.Entity = new ChartView()
            {
                Labels = months.ToList(),
                Values = listValue,
                Total = users.Count,
                TotalSell = doneCourses.Select(x => x.TotalPrice).Sum(),
                Sells = listSells,
                PaidSalary = listPaidSalary,
                Revenue = listRevenues,
                TotalRevenue = listRevenues.Sum()
            };
            return response;
        }

        public async Task<DataResult<int>> GetCountAllMember()
        {
            var response = new DataResult<int>();


            response.TotalRecords = await _unitOfWork.GetRepository<User>()
                .AsQueryable()
                .Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x=>x.UserRoles.Any(role=>role.Role.Name=="Member")).CountAsync();


            return response;
        }
    }
}
