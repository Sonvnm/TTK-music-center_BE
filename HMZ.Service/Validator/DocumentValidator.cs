
using System.ComponentModel.DataAnnotations;
using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.DocumentServices;

namespace HMZ.Service.Validator
{
    public class DocumentValidator : IValidator<DocumentQuery>
    {
        private readonly IDocumentService _documentService;
        public DocumentValidator(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(DocumentQuery entity, string? userName = null, bool? isUpdate = false)
        {
            if (entity == null)
            {
                return new List<ValidationResult>(){
                    new ValidationResult("Entity is null", new[] { nameof(entity) })
                };
            }
            // update
            if (isUpdate == true)
            {
                var course = await _documentService.GetByIdAsync(entity.Id.ToString());
                if (course.Entity == null)
                {
                    return new List<ValidationResult>(){
                        new ValidationResult("Không tìm thấy tài liệu", new[] { nameof(entity.Id) })
                    };
                }
            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.Name), entity.Name),
            };
            return await Task.FromResult(result);
        }
    }
}