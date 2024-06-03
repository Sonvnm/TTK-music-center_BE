using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
namespace HMZ.Database.Entities
{
    // Quá trình học tập
    public class LearningProcess : BaseEntity
    {   
        public string? Name { get; set; } // tên buổi học
        public string? Description { get; set; } // mô tả buổi học
        public string? Assets { get; set; }  // danh sách tài sản mượn trong buổi học, TaiSan1, TaiSan2, TaiSan3, ...
        public ELearningProcessStatus Status { get; set; } = ELearningProcessStatus.New;

        // Foreign key
        public Guid? UserId { get; set; }  // giáo viên dạy
        public User? User { get; set; }
        public Guid? ScheduleDetailId { get; set; } // chi tiet lịch học
        public ScheduleDetail? ScheduleDetail { get; set; }

        public Guid? ScheduleId { get; set; } // chi tiet lịch học

        public List<StudentStudyProcess>? StudentStudyProcesses { get; set; }
        
    }
}