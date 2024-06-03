using HMZ.Database.Entities.Base;
namespace HMZ.Database.Entities
{
    // Tài liệu
    public class Document : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? FileSize { get; set; }
        public string? FileExtension { get; set; }
        public string? FilePath { get; set; }
        public string? Thumbnail { get; set; }
        public bool IsPublic { get; set; } = true; // Có công khai không
        public string? PublicId { get; set; } // Public Id của Cloudinary

        // Foreign key
        public Guid? UserId { get; set; } // Người Tải lên
        public User? User { get; set; }
        public Guid? ClassId { get; set; } // Lớp học
        public Class? Class { get; set; }    
        public Guid? SubjectId { get; set; } // Lớp học
        public Subject? Subject { get; set; }

    }
}