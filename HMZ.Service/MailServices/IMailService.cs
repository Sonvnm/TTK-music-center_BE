using HMZ.DTOs.Queries;
using HMZ.Service.Helpers;

namespace HMZ.Service.MailServices
{
    public interface IMailService
    {
        Task<DataResult<bool>> SendEmailAsync(MailQuery mailRequest);
    }
}
