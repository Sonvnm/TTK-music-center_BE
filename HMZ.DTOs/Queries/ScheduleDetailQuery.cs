
using HMZ.Database.Entities.Base;
namespace HMZ.DTOs.Queries
{
    public class ScheduleDetailQuery: BaseEntity
    {
        public string? Name { get; set; } // Tên lịch học
        public Guid? ScheduleDetailId { get; set; } // Tên lịch học
        public Guid? ScheduleId { get; set; } // Lịch học
        public Guid? RoomId { get; set; } // Phòng học
        public DateTime? StartDate { get; set; } 
        public int? TimeSlot { get; set; } // 1. Sáng 7h-9h, 2. Chiều 13h-15h, 3. Tối 18h-20h
        public bool? IsMakeUpClass { get; set; } 
    }
}