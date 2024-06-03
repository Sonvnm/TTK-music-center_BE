using HMZ.DTOs.Filters.Base;
namespace HMZ.DTOs.Filters
{
    public class SubjectFilter : BaseFilter
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CourseId { get; set; }
    }
}