
using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
namespace HMZ.DTOs.Queries
{
    public class HistorySystemQuery : BaseEntity
    {
        public string? Action { get; set; }
        public string? Description { get; set; }
        public ETypeHistory? Type { get; set; }
        public Guid? UserId { get; set; }
        public string? Username { get; set; }
    }
}