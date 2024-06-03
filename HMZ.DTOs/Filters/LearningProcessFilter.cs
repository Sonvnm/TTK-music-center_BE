using HMZ.DTOs.Filters.Base;
using HMZ.DTOs.Models;
namespace HMZ.DTOs.Filters
{
    public class LearningProcessFilter : BaseFilter
    {
        public string? Name { get; set; }
        public RangeFilter<DateTime?>? StartTime { get; set; }
        public RangeFilter<DateTime?>? EndTime { get; set; } 
        public string? Description { get; set; } 
        public string? Assets { get; set; }  

        // Foreign key
        public string? Username { get; set; }  
        public string? ScheduleCode { get; set; }
    }
}