using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HMZ.Database.Entities;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Extensions;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;
using HMZ.Service.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HMZ.SDK.Extensions;
using HMZ.Database.Commons;

namespace HMZ.Service.Services.MessageServices
{
    public class MessageService : ServiceBase<IUnitOfWork>, IMessageService
    {
        public MessageService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(unitOfWork, serviceProvider)
        {
        }

        public async Task<DataResult<bool>> CreateAsync(MessageQuery entity)
        {
            var result = new DataResult<bool>();
            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<MessageQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }
            // Create entity

            var message = new Message
            {
                Content = entity.Content,
                SendAt = DateTime.Now,
                UserId = entity.UserId,
                ClassId = entity.ClassId,

                CreatedBy = entity.CreatedBy,
            };
            await _unitOfWork.GetRepository<Message>().Add(message);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                return result;
            }
            result.Errors.Add("Gửi tin nhắn thất bại");
            return result;
        }

        public async Task<DataResult<int>> DeleteAsync(string id)
        {
            var result = new DataResult<int>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }

            var message = await _unitOfWork.GetRepository<Message>().AsQueryable().FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
            if (message == null)
            {
                result.Errors.Add("Message not found");
                return result;
            }

            _unitOfWork.GetRepository<Message>().Delete(message, false);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                return result;
            }
            result.Errors.Add("Xóa tin nhắn thất bại");
            return result;
        }

        public Task<DataResult<int>> DeleteAsync(string[] id)
        {
            throw new NotImplementedException();
        }

        public async Task<DataResult<List<MessageView>>> GetByClassIdAsync(Guid classId, int page = 1, int pageSize = 20)
        {
            var result = new DataResult<List<MessageView>>();
            if (classId == Guid.Empty)
            {
                result.Errors.Add("ClassId is null or empty");
                return result;
            }

            var messages = await _unitOfWork.GetRepository<Message>().AsQueryable()
                .Include(x => x.User)
                .Where(x => x.ClassId == classId)
                .OrderBy(x => x.SendAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            result.Entity = messages.Select(x => new MessageView(x)
            {
                ClassId = x.ClassId,
                Content = x.Content,
                SendAt = x.SendAt,
                UserId = x.UserId,
                TimeAgo = HMZHelper.TimeAgo(x.SendAt.Value),
                User = new UserView()
                {
                    Email = x.User.Email,
                    Id = x.User.Id,
                    Username = x.User.UserName,
                    Image = x.User.Image,
                },

            }).ToList();
            return result;
        }

        public async Task<DataResult<MessageView>> GetByCodeAsync(string code)
        {
            var result = new DataResult<MessageView>();
            if (string.IsNullOrEmpty(code))
            {
                result.Errors.Add("Code is null or empty");
                return result;
            }

            var message = await _unitOfWork.GetRepository<Message>().AsQueryable().FirstOrDefaultAsync(x => x.Code == code);
            if (message == null)
            {
                result.Errors.Add("Message not found");
                return result;
            }

            result.Entity = new MessageView(message)
            {
                ClassId = message.ClassId,
                Content = message.Content,
                SendAt = message.SendAt,
                UserId = message.UserId,

            };
            return result;
        }

        public async Task<DataResult<MessageView>> GetByIdAsync(string id)
        {
            var result = new DataResult<MessageView>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }

            var message = await _unitOfWork.GetRepository<Message>().AsQueryable().FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
            if (message == null)
            {
                result.Errors.Add("Message not found");
                return result;
            }

            result.Entity = new MessageView(message)
            {
                ClassId = message.ClassId,
                Content = message.Content,
                SendAt = message.SendAt,
                UserId = message.UserId,

            };
            return result;
        }

        public async Task<DataResult<MessageView>> GetPageList(BaseQuery<MessageFilter> query)
        {
            var result = new DataResult<MessageView>();
            var roomQuery = _unitOfWork.GetRepository<Message>().AsQueryable()
                .Include(x => x.Class)
                .Include(x => x.User)
                .Where(x => x.IsActive == true)
                .ApplyFilter(query)
                .OrderByColumns(query.SortColumns, query.SortOrder);

            result.TotalRecords = await roomQuery.CountAsync();
            result.Items = await roomQuery
                    .Skip((query.PageNumber.Value - 1) * query.PageSize.Value)
                    .Take(query.PageSize.Value)
                    .Select(x => new MessageView(x)
                    {
                        ClassId = x.ClassId,
                        Content = x.Content,
                        SendAt = x.SendAt,
                        UserId = x.UserId,
                        User = new UserView()
                        {
                            Email = x.User.Email,
                            Id = x.User.Id,
                            Username = x.User.UserName,
                            Image = x.User.Image,
                        },
                        Class = new Class()
                        {
                            Id = x.Class.Id,
                            Name = x.Class.Name,
                            Code = x.Class.Code
                        }
                    }).ToListAsync();
            return result;
        }

        public async Task<DataResult<int>> UpdateAsync(MessageQuery entity, string id)
        {
            var result = new DataResult<int>();
            if (string.IsNullOrEmpty(id))
            {
                result.Errors.Add("Id is null or empty");
                return result;
            }

            var message = await _unitOfWork.GetRepository<Message>().AsQueryable().FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
            if (message == null)
            {
                result.Errors.Add("Message not found");
                return result;
            }

            // Validate entity
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<MessageQuery>>();
            List<ValidationResult> resultValidator = new List<ValidationResult>();
            if (validator != null)
                resultValidator = await validator.ValidateAsync(entity);
            if (resultValidator.HasError())
            {
                result.Errors.AddRange(resultValidator.JoinError());
                return result;
            }

            message.Content = entity.Content;
            message.SendAt = entity.SendAt;
            message.UserId = entity.UserId;
            message.ClassId = entity.ClassId;

            _unitOfWork.GetRepository<Message>().Update(message);
            if (await _unitOfWork.SaveChangesAsync() > 0)
            {
                return result;
            }
            result.Errors.Add("Cập nhật tin nhắn thất bại");
            return result;
        }
    }
}