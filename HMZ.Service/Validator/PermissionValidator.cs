using System.ComponentModel.DataAnnotations;
using HMZ.Database.Entities;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services;
using Microsoft.EntityFrameworkCore;

namespace HMZ.Service.Validator
{
    public class PermissionValidator : IValidator<PermissionQuery>
    {
        private readonly Services.PermissionServices.IPermissionService _permissonService;
        private readonly IUnitOfWork _unitOfWork;
        public PermissionValidator(Services.PermissionServices.IPermissionService permissonService, IUnitOfWork unitOfWork)
        {
            _permissonService = permissonService;
               _unitOfWork = unitOfWork;
        }

        public async Task<List<ValidationResult>> ValidateAsync(PermissionQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
            // check exist
            if (isUpdate != true)
            {
                var permission = await _unitOfWork.GetRepository<Permission>().AsQueryable().FirstOrDefaultAsync(x=>x.Value.Equals(entity.Value));
                if (permission != null)
                {
                    return new List<ValidationResult>(){
                    new ValidationResult("Permission is exist", new[] { nameof(entity.Key) })
                };
                }
            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.Key), entity.Key),
                ValidatorCustom.IsRequired(nameof(entity.Value), entity.Key),
            };
            return await Task.FromResult(result);
        }
    }
}