using HMZ.DTOs.Filters.Base;
namespace HMZ.DTOs.Filters
{
    public class ClassFilter: BaseFilter
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CourseName { get; set; }
    }
}