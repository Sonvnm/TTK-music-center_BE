
using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Base;

namespace HMZ.DTOs.Views
{
    public class StudentStudyProcessView : BaseView<StudentStudyProcess>
    {
        public StudentStudyProcessView(StudentStudyProcess entity) : base(entity)
        {
        }
        public string? Description { get; set; } 
        public bool? IsAbsent { get; set; }
        public bool? IsLate { get; set; }

        public string? Username { get; set; }
        public string? LearningProcessCode { get; set; }
        public string? LearningProcessName { get; set; }
        public Guid? LearningProcessId{ get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string LearningProcessStatus { get; set; }

        public string? RoomName{ get; set; }
    }
}