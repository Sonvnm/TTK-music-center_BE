
using HMZ.Database.Entities.Base;
namespace HMZ.DTOs.Queries
{
    public class StudentStudyProcessQuery : BaseEntity
    {
        public string? Description { get; set; } // Mô tả tất cả quá trình học tập của sinh viên trong buổi học
        public bool IsAbsent { get; set; } = false; // Sinh viên có vắng mặt không
        public bool? IsLate { get; set; } = false; // Sinh viên có đi muộn không
        public bool IsUpdate { get; set; } = false;

        // Foreign key
        public Guid? UserId { get; set; } 
        public Guid? LearningProcessId { get; set; }
    }
}