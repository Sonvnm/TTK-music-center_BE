using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.MessageServices
{
    public interface IMessageService : IBaseService<MessageQuery, MessageView, MessageFilter>
    {
        Task<DataResult<int>> DeleteAsync(string id);
        Task<DataResult<List<MessageView>>> GetByClassIdAsync(Guid classId,int page = 1, int pageSize = 20);
    }
}