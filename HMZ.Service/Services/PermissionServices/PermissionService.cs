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

namespace HMZ.Service.Services.PermissionServices
{
    public class PermissionService : ServiceBase<IUnitOfWork>, IPermissionService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        public PermissionService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider, UserManager<User> userManager, RoleManager<Role> roleManager) : base(unitOfWork, serviceProvider)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<DataResult<int>> AddToRolePermissionAsync(string roleCode, string[] permissionsId)
        {
            var result = new DataResult<int>();
            var role = await _unitOfWork.GetRepository<Role>().AsQueryable().FirstOrDefaultAsync(x => x.Code == roleCode);
            if (role == null)
            {
                result.Errors.Add("Role not found");
                return result;
            }
            var permissions = await _unitOfWork.GetRepository<Permission>().AsQueryable().Where(x => permissionsId.Contains(x.Id.ToString())).ToListAsync();
            if (permissions == null || permissions.Count == 0)
            {
                result.Errors.Add("Permission not found");
                return result;
            }
            var rolePermissions = permissions.Select(x => new RolePermission
            {
                PermissionId = x.Id.Value,
                RoleId = role.Id,
            }).ToList();
            await _unitOfWork.GetRepository<RolePermission>().AddRange(rolePermissions);
            result.Entity = await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<DataResult<bool>> CreateAsync(PermissionQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<PermissionQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }

            var permission = new Permission
            {
                Value = entity.Value,
                Key = entity.Key,
                Description = entity.Description,
                CreatedBy = entity.CreatedBy ?? "System",
            };
            await _unitOfWork.GetRepository<Permission>().Add(permission);
            result.Entity = await _unitOfWork.SaveChangesAsync() > 0;
            if (result.Entity == false)
            {
                result.Errors.Add("Error while saving");
                return result;
            }
            return result;
        }

        public async Task<DataResult<int>> DeleteAsync(string[] id)
        {
            var result = new DataResult<int>();
            if (id == null || id.Length == 0)
            {
                result.Errors.Add("Permission not found");
                return result;
            }
            var permissions = await _unitOfWork.GetRepository<Permission>().AsQueryable().Where(x => id.Contains(x.Id.ToString())).ToListAsync();
            if (permissions == null || permissions.Count == 0)
            {
                result.Errors.Add("Permission not found");
                return result;
            }
            // check role permission
            var rolePermissions = await _unitOfWork.GetRepository<RolePermission>().AsQueryable().Where(x => id.Contains(x.PermissionId.ToString())).ToListAsync();
            if (rolePermissions != null && rolePermissions.Count > 0)
            {
                // remove role permission
                _unitOfWork.GetRepository<RolePermission>().DeleteRange(rolePermissions, false);
                if (await _unitOfWork.SaveChangesAsync() <= 0)
                {
                    result.Errors.Add("Error while saving role permission");
                    return result;
                }
            }
            _unitOfWork.GetRepository<Permission>().DeleteRange(permissions, false);
            result.Entity += await _unitOfWork.SaveChangesAsync();
            result.Message = "Delete permission success";
            return result;

        }

        public async Task<DataResult<PermissionView>> GetAllRolePermissionsAsync(BaseQuery<PermissionFilter> query)
        {
            var response = new DataResult<PermissionView>();
            var permissionQuery =  _unitOfWork.GetRepository<RolePermission>().AsQueryable()
                .Include(x => x.Permission)
                .Include(x => x.Role)
                .ApplyFilter(query)
                .OrderByColumns(query.SortColumns, query.SortOrder);

            response.TotalRecords = await permissionQuery.CountAsync();
            response.Items = await permissionQuery.Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                .Take(query.PageSize.Value)
                .Select(x => new PermissionView(x.Permission)
                {
                    Id = x.Id,
                    Key = x.Permission.Key,
                    PermissionId = x.Permission.Id,
                    RoleId = x.Role.Id,
                    Value = x.Permission.Value,
                    Description = x.Permission.Description,
                    RoleName = x.Role.Name,
                })
                .ToListAsync();
            return response;
        }

        public async Task<DataResult<PermissionView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<PermissionView>();
            var permission = await _unitOfWork.GetRepository<Permission>().GetByCondition(x => x.Code == code).FirstOrDefaultAsync();
            if (permission == null)
            {
                result.Errors.Add("Permission not found");
                return result;
            }
            result.Entity = new PermissionView(permission)
            {
                Id = permission.Id,
                Key = permission.Key,
                Value = permission.Value,
                Description = permission.Description,

            };
            return result;
        }

        public async Task<DataResult<PermissionView>> GetByIdAsync(string id)
        {
            var result = new DataResult<PermissionView>();
            var permission = await _unitOfWork.GetRepository<Permission>().GetByIdAsync(Guid.Parse(id));
            if (permission == null)
            {
                result.Errors.Add("Permission not found");
                return result;
            }
            result.Entity = new PermissionView(permission)
            {
                Id = permission.Id,
                Key = permission.Key,
                Value = permission.Value,
                Description = permission.Description,

            };
            return result;
        }

        public async Task<DataResult<RoleView>> GetByRoleAsync(Guid roleId)
        {
            var response = new DataResult<RoleView>();
            var permission = await _unitOfWork.GetRepository<Permission>().AsQueryable()
                .Include(x => x.RolePermissions)
                .Where(x => x.RolePermissions.Any(y => y.RoleId == roleId))
                .Select(x => new RoleView()
                {
                    Name = x.RolePermissions.FirstOrDefault().Role.Name,
                    Permissions = x.RolePermissions.Select(y => new PermissionView(y.Permission)
                    {
                        Id = y.Permission.Id,
                        Key = y.Permission.Key,
                        Value = y.Permission.Value,
                        Description = y.Permission.Description,


                    }).ToList(),
                }).ToListAsync();
            response.Items = permission;
            return response;
        }

        public async Task<DataResult<RoleView>> GetByRoleAsync(string Name)
        {
            var response = new DataResult<RoleView>();
            var permission = await _unitOfWork.GetRepository<RolePermission>().AsQueryable()
            .Include(x => x.Permission)
            .Include(x => x.Role)
            .Select(x => new RoleView()
            {
                Name = x.Role.Name,
                Permissions = x.Role.RolePermissions.Where(y => y.Role.Name == Name).Select(y => new PermissionView(y.Permission)
                {
                    Id = y.Permission.Id,
                    Key = y.Permission.Key,
                    Value = y.Permission.Value,
                    Description = y.Permission.Description,

                }).ToList(),
            }).FirstOrDefaultAsync(x => x.Name == Name);
            response.Entity = permission;
            return response;

        }

        public async Task<DataResult<PermissionView>> GetByRoleAsync(BaseQuery<PermissionFilter> filter)
        {
            var response = new DataResult<PermissionView>();
            if (string.IsNullOrEmpty(filter?.Entity?.RoleCode))
            {
                response?.Errors.Add("Role name is required");
                return response;
            }
            string roleCode = filter.Entity.RoleCode;
            filter.Entity.RoleCode = null;

            var permissionQuery = _unitOfWork.GetRepository<RolePermission>().AsQueryable()
                .Include(x => x.Permission)
                .Include(x => x.Role)
                .Where(x => x.Role.Code == roleCode)
                .ApplyFilter(filter)
                .OrderByColumns(filter.SortColumns, filter.SortOrder);

            response.TotalRecords = await permissionQuery.CountAsync();
            response.Items = await permissionQuery.Select(x => new PermissionView(x.Permission)
            {
                Id = x.Permission.Id,
                Key = x.Permission.Key,
                Value = x.Permission.Value,
                Description = x.Permission.Description,

                RoleName = x.Role.Name,
                RoleCode = x.Role.Code,
                RoleId = x.Role.Id,
            }).ToListAsync();

            return response;
        }

        public async Task<DataResult<UserView>> GetByUserAsync(Guid userId)
        {
            var response = new DataResult<UserView>();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }
            // Get all roles of the user and their permissions
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _unitOfWork.GetRepository<RolePermission>()
                .AsQueryable()
                .Include(rp => rp.Permission)
                .Include(rp => rp.Role)
                .Where(rp => roles.Contains(rp.Role.Name))
                .Select(rp => rp.Permission)
                .ToListAsync();
            // Merge all permissions into a single array
            var allPermissions = permissions.Select(p => new PermissionView(p)
            {
                Id = p.Id,
                Key = p.Key,
                Value = p.Value,
                Description = p.Description,


            }).ToList();
            // build the user view model
            response.Entity = new UserView()
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                RolesView = new List<RoleView>()
                {
                    new RoleView()
                    {
                        Name = roles.FirstOrDefault(),
                        Permissions = allPermissions
                    }
                }
            };

            return response;
        }

        public async Task<DataResult<UserView>> GetByUserAsync(string username)
        {

            var response = new DataResult<UserView>();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                response.Errors.Add("User not found");
                return response;
            }

            // Get all roles of the user and their permissions
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _unitOfWork.GetRepository<RolePermission>()
                .AsQueryable()
                .Include(rp => rp.Permission)
                .Include(rp => rp.Role)
                .Where(rp => roles.Contains(rp.Role.Name))
                .Select(rp => rp.Permission)
                .ToListAsync();

            // Merge all permissions into a single array
            var allPermissions = permissions.Select(p => new PermissionView(p)
            {
                Id = p.Id,
                Key = p.Key,
                Value = p.Value,
                Description = p.Description,

            }).ToList();

            // Build the UserView object
            var userView = new UserView()
            {
                Id = user.Id,
                Username = user.UserName,
                RolesView = new List<RoleView>()
                {
                    new RoleView()
                    {
                        Name = string.Join(", ", roles),
                        Permissions = allPermissions
                    }
                }
            };

            response.Entity = userView;
            return response;
        }

        public async Task<DataResult<PermissionView>> GetNotInRoleAsync(BaseQuery<PermissionFilter> filter)
        {
            var response = new DataResult<PermissionView>();
            if (string.IsNullOrEmpty(filter?.Entity?.RoleCode))
            {
                response?.Errors.Add("Role name is required");
                return response;
            }
            string roleCode = filter.Entity.RoleCode;
            filter.Entity.RoleCode = null;

            // get all permissions not contains in rolePermissions
            var permissionQuery = _unitOfWork.GetRepository<Permission>()
               .AsQueryable()
               .Include(x => x.RolePermissions)
               .Where(x => !x.RolePermissions.Any(y => y.Role.Code == roleCode))
               .ApplyFilter(filter)
               .OrderByColumns(filter.SortColumns, filter.SortOrder);

            response.Items = await permissionQuery
            .Select(x => new PermissionView(x)
            {
                Id = x.Id,
                Key = x.Key,
                Value = x.Value,
                Description = x.Description,
            }).ToListAsync();

            response.TotalRecords = await permissionQuery.CountAsync();

            return response;
        }

        public async Task<DataResult<PermissionView>> GetPageList(BaseQuery<PermissionFilter> query)
        {
            var response = new DataResult<PermissionView>();
            var permissionQuery = _unitOfWork.GetRepository<Permission>().AsQueryable()
                    .ApplyFilter(query)
                    .OrderByColumns(query.SortColumns, query.SortOrder);

            response.TotalRecords = await permissionQuery.CountAsync();
            response.Items = await permissionQuery.Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new PermissionView(x)
                    {
                        Id = x.Id,
                        Key = x.Key,
                        Value = x.Value,
                        Description = x.Description,
                    }).ToListAsync();
            return response;
        }


        public async Task<DataResult<int>> RemoveRolePermissionAsync(string code, string[] permissionsId)
        {
            var result = new DataResult<int>();
            var role = await _unitOfWork.GetRepository<Role>().AsQueryable().FirstOrDefaultAsync(x => x.Code == code);
            if (role == null)
            {
                result.Errors.Add("Role not found");
                return result;
            }
            var rolePermissions = await _unitOfWork.GetRepository<RolePermission>().AsQueryable()
                .Where(x => x.RoleId == role.Id && permissionsId.Contains(x.PermissionId.ToString())).ToListAsync();
            if (rolePermissions == null || rolePermissions.Count == 0)
            {
                result.Errors.Add("Role permission not found");
                return result;
            }
            _unitOfWork.GetRepository<RolePermission>().DeleteRange(rolePermissions, false);
            result.Entity = await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(PermissionQuery entity, string id)
        {
            var result = new DataResult<int>();
            // Validate
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<PermissionQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }

            var permission = await _unitOfWork.GetRepository<Permission>().GetByIdAsync(Guid.Parse(id));
            if (permission == null)
            {
                result.Errors.Add("Permission not found");
                return result;
            }
            permission.Value = entity.Value;
            permission.Key = entity.Key;
            permission.Description = entity.Description;

            permission.UpdatedAt = DateTime.Now;
            permission.UpdatedBy = entity.UpdatedBy;

            _unitOfWork.GetRepository<Permission>().Update(permission);
            result.Entity = await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<DataResult<bool>> UpdateRolePermissionAsync(PermissionQuery entity, Guid roleId, Guid permissionId)
        {
            var result = new DataResult<bool>();
            var rolePermission = await _unitOfWork.GetRepository<RolePermission>().AsQueryable()
                .FirstOrDefaultAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);
            if (rolePermission == null)
            {
                result.Errors.Add("Role permission not found");
                return result;
            }
            rolePermission.PermissionId = entity.PermissionId.Value;
            rolePermission.RoleId = entity.RoleId.Value;

            rolePermission.IsActive = entity.IsActive;
            rolePermission.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<RolePermission>().Update(rolePermission);
            result.Entity = await _unitOfWork.SaveChangesAsync() > 0;
            return result;
        }
    }
}
