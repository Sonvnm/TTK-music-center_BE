using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
namespace HMZ.DTOs.Queries
{
    public class LearningProcessQuery : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Assets { get; set; } 
        public Guid? UserId { get; set; } 
        public Guid? ScheduleDetailId { get; set; }
        public Guid? ScheduleId { get; set; }
        public ELearningProcessStatus? Status { get; set; }
    }
}