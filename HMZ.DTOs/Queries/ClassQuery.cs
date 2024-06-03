using HMZ.Database.Entities.Base;
namespace HMZ.DTOs.Queries
{
    public class ClassQuery : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? CourseId { get; set; }
        
    }
}