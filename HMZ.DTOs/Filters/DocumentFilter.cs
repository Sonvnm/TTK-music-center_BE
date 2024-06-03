using HMZ.DTOs.Filters.Base;
using HMZ.DTOs.Models;
namespace HMZ.DTOs.Filters
{
    public class DocumentFilter : BaseFilter
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public RangeFilter<int?>? FileSize { get; set; }
        public string? FileExtension { get; set; }
        public string? FilePath { get; set; }
        public string? Thumbnail { get; set; }
        public bool? IsPublic { get; set; }

        public string? Username { get; set; }
        public string? ClassName { get; set; }
        public string? SubjectName { get; set; }

        // For Get By Class
        public string? ClassCode { get; set; }
        public string? ClassId { get; set; }
        public string? CourseId { get; set; }
        public string? SubjectCode { get; set; }

    }
}