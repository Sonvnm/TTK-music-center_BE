using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
namespace HMZ.Database.Entities
{
    // Lich dạng dạy
    public class Schedule : BaseEntity
    {
        public string? Name { get; set; } // Tên lịch học
        public string? Description { get; set; } // Mô tả lịch học
        public DateTime? StartDate { get; set; } // Ngày bắt đầu
        public DateTime? EndDate { get; set; } // Ngày kết thúc

        // Foreign key
        public Guid? CourseId { get; set; } // Khóa học
        public Course? Course { get; set; }

        public Guid? ClassId { get; set; } // Khóa học
        public Class? Class { get; set; }

        public EScheduleStatus Status { get; set; } = EScheduleStatus.Accepted; // Trạng thái lịch học
        public List<LearningProcess>? LearningProcesses { get; set; }
        public List<ScheduleDetail>? ScheduleDetails { get; set; }
    }
}