using HMZ.Database.Enums;
using HMZ.DTOs.Filters.Base;
using HMZ.DTOs.Models;

namespace HMZ.DTOs.Filters
{
    public class ScheduleFilter:BaseFilter
    {
        public string? Name { get; set; }
        public EScheduleStatus? Status { get; set; }
        public string? CourseName { get; set; }
        public string? ClassName { get; set; }
        public RangeFilter<DateTime?>? StartDate { get; set; }
        public RangeFilter<DateTime?>? EndDate { get; set; }

        //other
        public Guid? CourseId { get; set; }
    }
}
