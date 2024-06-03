using HMZ.Database.Entities.Base;
namespace HMZ.Database.Entities
{
    public class Message : BaseEntity
    {
        public string? Content { get; set; }
        public DateTime? SendAt { get; set; } = DateTime.Now;
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public Guid? ClassId { get; set; }
        public Class? Class { get; set; }
    }
}