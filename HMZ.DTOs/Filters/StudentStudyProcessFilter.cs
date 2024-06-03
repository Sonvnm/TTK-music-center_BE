
using HMZ.DTOs.Filters.Base;
namespace HMZ.DTOs.Filters
{
    public class StudentStudyProcessFilter: BaseFilter
    {
        public string? Description { get; set; }
        public bool? IsAbsent { get; set; }
        public bool? IsLate { get; set; }

        public string? Username { get; set; }
        public string? LearningProcessCode { get; set; }
        public string? LearningProcessName { get; set; }
    }
}