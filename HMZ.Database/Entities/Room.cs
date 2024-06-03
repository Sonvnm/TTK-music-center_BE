using HMZ.Database.Entities.Base;
namespace HMZ.Database.Entities
{
    // Phòng học
    public class Room : BaseEntity
    {
        public string? Name { get; set; } // Tên phòng
        public string? Description { get; set; }

        // Foreign key
        public virtual List<Schedule>? Schedules { get; set; }
    }
}