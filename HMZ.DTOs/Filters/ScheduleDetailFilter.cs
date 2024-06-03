
using HMZ.DTOs.Filters.Base;
using HMZ.DTOs.Models;

namespace HMZ.DTOs.Filters
{
    public class ScheduleDetailFilter: BaseFilter
    {
        public string? Name { get; set; }
        public RangeFilter<DateTime?>? StartTime { get; set; }
        public RangeFilter<DateTime?>? EndTime { get; set; }
        public string? RoomName { get; set; }
        public Guid? ScheduleId { get; set; }
    }
}