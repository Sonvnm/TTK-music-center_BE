using HMZ.Database.Entities.Base;
using Microsoft.AspNetCore.Http;
namespace HMZ.DTOs.Queries
{
    public class DocumentQuery : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? FileSize { get; set; }
        public string? FileExtension { get; set; }
        public string? FilePath { get; set; }
        public string? Thumbnail { get; set; }
        public bool IsPublic { get; set; } = true;
         public string? PublicId { get; set; } // Public Id của Cloudinary

        public Guid? UserId { get; set; }
        public Guid? ClassId { get; set; }

        // for upload
        public IFormFile? File { get; set; }
        public string? ClassCode { get; set; }
        public string? SubjectCode { get; set; }
    }
}