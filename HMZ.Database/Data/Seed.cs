using HMZ.Database.Entities;
using HMZ.Database.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace HMZ.Database.Data
{
    public class Seed
    {
        public async static Task<int> SeedUser(UserManager<User> userManager, RoleManager<Role> roleManager, HMZContext context)
        {
            if (userManager.Users.Any())
            {
                return 0;
            }
            string path = Directory.GetCurrentDirectory() + "\\Resources\\";

            var users = await File.ReadAllTextAsync(path + "UserSeedData.json");
            var usersToSeed = System.Text.Json.JsonSerializer.Deserialize<List<User>>(users);
            var roles = new List<Role>
            {
                new Role(){ Name=EUserRoles.Admin.ToString()},
                new Role(){ Name=EUserRoles.Member.ToString()},
                new Role(){ Name=EUserRoles.Teacher.ToString()},

            };

            // add permission
            var permissions = await File.ReadAllTextAsync(path + "PermissionSeedData.json");
            var permissionsToSeed = System.Text.Json.JsonSerializer.Deserialize<List<Permission>>(permissions);
            foreach (var permission in permissionsToSeed)
            {
                await context.Permissions.AddAsync(permission);
            }
            await context.SaveChangesAsync();

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);

                // add permission to role admin
                if (role.Name.Equals(EUserRoles.Admin.ToString()))
                {
                    context.RolePermissions.AddRange(permissionsToSeed.Select(x => new RolePermission()
                    {
                        RoleId = role.Id,
                        PermissionId = x.Id.Value,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                    }));
                }

            }
            await context.SaveChangesAsync();


            // add user
            foreach (var user in usersToSeed)
            {
                user.UserName = user.UserName.ToLower();
                user.CreatedAt = DateTime.Now;
                user.DateOfBirth = DateTime.Now;
                user.IsActive = true;
                user.CreatedBy = "System";
                await userManager.CreateAsync(user, "Abc12345@");
                if (user.UserName.Equals(EUserRoles.Admin.ToString().ToLower()))
                {
                    await userManager.AddToRoleAsync(user, EUserRoles.Admin.ToString());
                    await userManager.AddToRoleAsync(user, EUserRoles.Teacher.ToString());

                }
                else if (user.UserName.Equals(EUserRoles.Teacher.ToString().ToLower()))
                {
                    await userManager.AddToRoleAsync(user, EUserRoles.Teacher.ToString());
                }
                else
                {
                    await userManager.AddToRoleAsync(user, EUserRoles.Member.ToString());
                }

            };
            return usersToSeed.Count;
        }
    }
}
