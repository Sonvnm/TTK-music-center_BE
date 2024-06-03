
using System.ComponentModel.DataAnnotations;

using HMZ.DTOs.Queries;
using HMZ.Service.Extensions;
using HMZ.Service.Services.MessageServices;

namespace HMZ.Service.Validator
{
    public class MessageValidator : IValidator<MessageQuery>
    {
        private readonly IMessageService _messageService;
        public MessageValidator(IMessageService messageService)
        {
            _messageService = messageService;
        }
        public async Task<List<ValidationResult>> ValidateAsync(MessageQuery entity, string? userName = null, bool? isUpdate = false)
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
                var message = await _messageService.GetByIdAsync(entity.Id.ToString());
                if (message.Entity == null)
                {
                    return new List<ValidationResult>(){
                        new ValidationResult("Không tìm message", new[] { nameof(entity.Id) })
                    };
                }
            }

            var result = new List<ValidationResult>(){
                ValidatorCustom.IsRequired(nameof(entity.Content), entity.Content),
                ValidatorCustom.IsRequired(nameof(entity.UserId), entity.UserId.ToString()),
                ValidatorCustom.IsRequired(nameof(entity.ClassId), entity.ClassId.ToString()),
            };
            return await Task.FromResult(result);
        }
    }
}