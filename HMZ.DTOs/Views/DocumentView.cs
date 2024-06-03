
using HMZ.Database.Entities;
using HMZ.DTOs.Base;

namespace HMZ.DTOs.Views
{
    public class DocumentView : BaseView<Document>
    {
        public DocumentView(Document entity) : base(entity)
        {
        }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? FileSize { get; set; }
        public string? FileExtension { get; set; }
        public string? FilePath { get; set; }
        public string? Thumbnail { get; set; }
        public bool IsPublic { get; set; }
         public string? PublicId { get; set; } // Public Id của Cloudinary

        public string? Username { get; set; }
        public string? ClassName { get; set; }
        public string? SubjectName { get; set; }
    }
}