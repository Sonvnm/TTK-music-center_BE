
using HMZ.Database.Entities;
using HMZ.DTOs.Base;
namespace HMZ.DTOs.Views
{
    public class LearningProcessView : BaseView<LearningProcess>
    {
        public LearningProcessView(LearningProcess entity) : base(entity)
        {
        }

        public Guid? ClassId { get; set; }
        public string? Name { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Description { get; set; }
        public string? Assets { get; set; }

        public string? Username { get; set; }
        public string? ClassName { get; set; }
        public string? ScheduleCode { get; set; }
        public string? ScheduleName { get; set; }
        public string? Status { get; set; }

        public string? CourseName { get; set; }

        public ScheduleDetail ScheduleDetail { get; set; }
    }
}