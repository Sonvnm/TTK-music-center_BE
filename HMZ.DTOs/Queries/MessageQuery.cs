
using HMZ.Database.Entities.Base;

namespace HMZ.DTOs.Queries
{
    public class MessageQuery: BaseEntity
    {
        public string? Content { get; set; }
        public DateTime? SendAt { get; set; } = DateTime.Now;
        public Guid? UserId { get; set; }
        public Guid? ClassId { get; set; }
    }
}