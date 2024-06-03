using HMZ.Database.Entities;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HMZ.Service.Validator
{
    public class LoginValidator : IValidator<LoginQuery>
    {
        private readonly UserManager<User> _userManager;
        public LoginValidator(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<List<ValidationResult>> ValidateAsync(LoginQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
            var result = new List<ValidationResult>(){
                // Email or Username
                ValidatorCustom.IsRequired(nameof(entity.Email), entity.Email),
                // Password
                ValidatorCustom.IsRequired(nameof(entity.Password), entity.Password),
            };

            // check email exist

            var user = entity.Email.Contains("@") ? await _userManager.FindByEmailAsync(entity.Email) : await _userManager.FindByNameAsync(entity.Email);
            if (user == null)
            {
                result.Add(new ValidationResult("Email or Username does not exist", new[] { nameof(entity.Email) }));
            }
            return await Task.FromResult(result);
        }

    }
}
