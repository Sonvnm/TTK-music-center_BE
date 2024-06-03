using Microsoft.AspNetCore.Http;

namespace HMZ.Service.Services.FileServices
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file);
        bool DeleteFile(string fileName);

    }
}
