using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace HMZ.Service.Services.CloudinaryServices
{
    public interface ICloudinaryService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file, string? folderName, bool isCrop = false, int width = 300, int height = 300);
        Task<UploadResult> UploadFile(IFormFile file, string? folderName);
        Task<DeletionResult> DeleteImageAsync(string publicId);
        Task<DeletionResult> DeleteFileAsync(string publicId);
    }
}