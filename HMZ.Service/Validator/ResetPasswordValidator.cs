using System.ComponentModel.DataAnnotations;
using HMZ.Database.Entities;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using Microsoft.AspNetCore.Identity;

namespace HMZ.Service.Validator
{
    public class ResetPasswordValidator : IValidator<UpdatePasswordQuery>
    {
        private readonly UserManager<User> _userManager;
        public ResetPasswordValidator(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<List<ValidationResult>> ValidateAsync(UpdatePasswordQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
            var result = new List<ValidationResult>(){

                ValidatorCustom.IsRequired(nameof(entity.NewPassword), entity.NewPassword),
                ValidatorCustom.Password(nameof(entity.NewPassword), entity.NewPassword),

                ValidatorCustom.IsRequired(nameof(entity.ConfirmPassword), entity.ConfirmPassword),
                ValidatorCustom.Password(nameof(entity.ConfirmPassword), entity.ConfirmPassword),
                ValidatorCustom.ConfirmPassword(nameof(entity.ConfirmPassword), entity.ConfirmPassword, entity.NewPassword),
            };
            // check tokem is valid
            string username = entity.Email.Split('@')[0].ToLower();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                result.Add(new ValidationResult("Email is not exist", new[] { nameof(entity.Email) }));
            }

            return await Task.FromResult(result);
        }
    }
}