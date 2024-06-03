using HMZ.Database.Entities.Base;
namespace HMZ.Database.Entities
{
    public class Class : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        
        // Foreign key
        public Guid? CourseId { get; set; }
        public Course? Course { get; set; }
        public virtual List<Document>? Documents { get; set; }
        public virtual List<Schedule>? Schedules { get; set; }
        public virtual List<StudentClass>? StudentClasses { get; set; }
        public virtual List<Message>? Messages { get; set; }
        
    }
}