using HMZ.DTOs.Filters.Base;
using HMZ.DTOs.Models;
namespace HMZ.DTOs.Filters
{
    public class CourseFilter : BaseFilter
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }
        public RangeFilter<decimal?>? Price { get; set; }
        public RangeFilter<DateTime?>? StartDate { get; set; }
        public RangeFilter<DateTime?>? EndDate { get; set; }
        public bool? Status { get; set; }
    }
}