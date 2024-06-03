using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
namespace HMZ.Database.Entities
{
    //Người trong Lớp học
    public class StudentClass : BaseEntity
    {
        public EPersonRoles Role  { get; set; } = EPersonRoles.Student;
        // Foreign key
        public Guid? UserId { get; set; } // Sinh viên or giáo viên
        public User? User { get; set; }
        public Guid? ClassId { get; set; } // Lớp học
        public Class? Class { get; set; }

        // List Message của lớp học
        public ICollection<Message>? Messages { get; set; }
    }
}