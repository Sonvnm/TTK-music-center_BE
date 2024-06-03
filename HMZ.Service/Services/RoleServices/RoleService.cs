using System.ComponentModel.DataAnnotations;
using HMZ.Database.Entities;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.SDK.Extensions;
using HMZ.Service.Extensions;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;
using HMZ.Service.Validator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HMZ.Service.Services.RoleServices
{
    public class RoleService : ServiceBase<IUnitOfWork>, IRoleService
    {

        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider, RoleManager<Role> roleManager, UserManager<User> userManager) : base(unitOfWork, serviceProvider)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }



        public async Task<DataResult<bool>> CreateAsync(RoleQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<RoleQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            var role = new Role
            {
                Name = entity.Name,
                NormalizedName = entity.Name.ToUpper(),
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = "System"
            };
            var resultCreate = await _roleManager.CreateAsync(role);
            if (!resultCreate.Succeeded)
            {
                result.Errors.Add("Create role failed");
                return result;
            }
            result.Entity = true;
            return result;
        }

        public async Task<DataResult<int>> DeleteAsync(string[] id)
        {
            var result = new DataResult<int>();
            if (id == null || id.Length == 0)
            {
                result.Errors.Add("Role not found");
                return result;
            }
            var roles = await _roleManager.Roles.Where(x => id.Contains(x.Id.ToString())).ToListAsync();
            if (roles == null || roles.Count == 0)
            {
                result.Errors.Add("Role not found");
                return result;
            }
            foreach (var role in roles)
            {
                var resultDelete = await _roleManager.DeleteAsync(role);
                if (!resultDelete.Succeeded)
                {
                    result.Errors.Add("Delete role failed");
                    result.Entity += 0;
                    continue;
                }
            }
            return result;
        }
        public async Task<DataResult<RoleView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<RoleView>();
            var role = await _unitOfWork.GetRepository<Role>().AsQueryable().FirstOrDefaultAsync(x => x.Code == code);
            if (role == null)
            {
                result.Errors.Add("Role not found");
                return result;
            }
            result.Entity = new RoleView
            {
                Id = role.Id.ToString(),
                Name = role.Name,
                Code = role.Code,
                IsActive = role.IsActive,

                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedBy,
                UpdatedAt = role.UpdatedAt,
                UpdatedBy = role.UpdatedBy
            };
            return result;
        }
        public async Task<DataResult<RoleView>> GetByIdAsync(string id)
        {
            var result = new DataResult<RoleView>();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                result.Errors.Add("Role not found");
                return result;
            }
            result.Entity = new RoleView
            {
                Id = role.Id.ToString(),
                Name = role.Name,
                Code = role.Code,
                IsActive = role.IsActive,

                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedBy,
                UpdatedAt = role.UpdatedAt,
                UpdatedBy = role.UpdatedBy
            };
            return result;
        }
        public async Task<RoleView> GetByNameAsync(string name)
        {
            var role = await _roleManager.Roles
                .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
                .Where(x => x.Name == name)
                .Select(r => new RoleView()
                {
                    Id = r.Id.ToString(),
                    Name = r.Name,
                    Code = r.Code,
                    IsActive = r.IsActive,
                    Permissions = r.RolePermissions.Select(x => x.Permission).Select(p => new PermissionView(p)
                    {
                        Id = p.Id,
                        PermissionId = p.Id.Value,
                        RoleId = r.Id,
                        Code = p.Code,
                        IsActive = p.IsActive,
                        Description = p.Description,
                        Key = p.Key,
                        Value = p.Value,
                        RoleName = r.Name,
                    }).ToList(),
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.CreatedBy,
                    UpdatedAt = r.UpdatedAt,
                    UpdatedBy = r.UpdatedBy
                })
                .FirstOrDefaultAsync();
            return role;

        }
        public async Task<DataResult<RoleView>> GetPageList(BaseQuery<RoleFilter> query)
        {
            var response = new DataResult<RoleView>();
            var roleQuery = _roleManager.Roles.AsQueryable()
                    .ApplyFilter(query)
                    .OrderByColumns(query.SortColumns, query.SortOrder);

            response.TotalRecords = await roleQuery.CountAsync();
            response.Items = await roleQuery.Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
            .Take(query.PageSize.Value)
            .Select(x => new RoleView
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Code = x.Code,
                IsActive = x.IsActive,

                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            }).ToListAsync(); 
            
            return response;
        }
        public async Task<DataResult<int>> UpdateAsync(RoleQuery entity, string id)
        {
            var result = new DataResult<int>();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                result.Errors.Add("Role not found");
                return result;
            }
            // Validate
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<RoleQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity, isUpdate: true);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            role.Name = entity.Name;
            role.NormalizedName = entity.Name.ToUpper();
            role.IsActive = entity.IsActive;

            role.UpdatedAt = DateTime.Now;
            role.UpdatedBy = entity.UpdatedBy;

            var resultUpdate = await _roleManager.UpdateAsync(role);
            if (!resultUpdate.Succeeded)
            {
                result.Errors.Add("Update role failed");
                return result;
            }
            result.Entity = 1;
            return result;
        }


        public async Task<DataResult<RoleView>> GetRolesByUsernameAsync(string username)
        {
            var result = new DataResult<RoleView>();
            var roles = await _roleManager.Roles.AsQueryable()
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.User)
            .Where(x => x.UserRoles.Any(x => x.User.UserName == username))
            .Select(x => new RoleView
            {
                Id = x.Id.ToString(),
                Name = x.Name,
                Code = x.Code,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy
            })
            .ToListAsync();
            result.Items = roles;
            return result;
        }
        public async Task<DataResult<int>> RemoveUserFromRoleAsync(string username, string roleName)
        {
            var result = new DataResult<int>();
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                result.Errors.Add("Role not found");
                return result;
            }
            var user = _userManager.Users.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                result.Errors.Add("User not found");
                return result;
            }
            var resultRemove = await _userManager.RemoveFromRoleAsync(user, role.Name);
            if (!resultRemove.Succeeded)
            {
                result.Errors.AddRange(resultRemove.Errors.Select(x => x.Description));
                return result;
            }
            result.Entity = 1;
            return result;

        }
        public async Task<DataResult<int>> AddUserToRoleAsync(string username, string roleName)
        {
            var result = new DataResult<int>();
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                result.Errors.Add("Role not found");
                return result;
            }
            var user = _userManager.Users.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                result.Errors.Add("User not found");
                return result;
            }
            var resultAdd = await _userManager.AddToRoleAsync(user, role.Name);
            if (!resultAdd.Succeeded)
            {
                result.Errors.AddRange(resultAdd.Errors.Select(x => x.Description));
                return result;
            }
            result.Entity = 1;
            return result;

        }
    }
}