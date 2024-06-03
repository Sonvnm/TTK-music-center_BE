
using HMZ.Database.Entities.Base;

namespace HMZ.Database.Entities
{
    // Quá trình học tập của sinh viên
    public class StudentStudyProcess : BaseEntity
    {
        public string? Description { get; set; } // Mô tả tất cả quá trình học tập của sinh viên trong buổi học
        public bool? IsAbsent { get; set; } = false; // Sinh viên có vắng mặt không
        public bool? IsLate { get; set; } = false; // Sinh viên có đi muộn không

        // Foreign key
        public Guid? UserId { get; set; } // Sinh viên
        public User? User { get; set; }
        public Guid? LearningProcessId { get; set; } // Quá trình học tập
        public LearningProcess? LearningProcess { get; set; }

    }
}