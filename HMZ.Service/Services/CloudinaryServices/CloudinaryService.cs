using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HMZ.Service.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HMZ.Service.Services.CloudinaryServices
{
    public class CloudinaryService : ICloudinaryService
    {
        private Cloudinary _cloudinary;
        public CloudinaryService(IOptions<CloudinarySetting> config)
        {
            Account account = new Account
            {
                Cloud = config.Value.CloudName,
                ApiKey = config.Value.APIKey,
                ApiSecret = config.Value.APISecret
            };
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<DeletionResult> DeleteFileAsync(string publicId)
        {
            var delete = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(delete);
            return result;
        }

        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var delete = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(delete);
            return result;
        }

        public async Task<UploadResult> UploadFile(IFormFile file, string? folderName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folderName,
                    PublicId = Guid.NewGuid().ToString()
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult;
            }
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string? folderName, bool isCrop = false, int width = 300, int height = 300)
        {
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    Folder = folderName,
                    File = new FileDescription(file.FileName, stream),
                    Transformation = isCrop ? new Transformation().Width(width).Height(height).Crop("fill") : null
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);

            }
            return uploadResult;
        }
    }
}