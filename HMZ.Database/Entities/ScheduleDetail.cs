
using HMZ.Database.Entities.Base;

namespace HMZ.Database.Entities
{
    public class ScheduleDetail : BaseEntity
    {
        public string? Name { get; set; } // Tên lịch học
        public Guid? ScheduleId { get; set; } // Lịch học
        public Guid? RoomId { get; set; } // Phòng học

        public DateTime? StartTime { get; set; } // Thời gian bắt đầu
        public DateTime? EndTime { get; set; } // Thời gian kết thúc
        public Schedule? Schedule { get; set; }

        public bool? IsMakeUpClass { get; set; }//Học bù

        // Foreign key
        public Room? Room { get; set; }
        public List<LearningProcess>? LearningProcesses { get; set; }
    }
}