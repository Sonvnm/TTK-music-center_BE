using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
namespace HMZ.Database.Entities
{
    // Lịch sử hệ thống
    public class HistorySystem : BaseEntity
    {
        public string? Action { get; set; }
        public string? Description { get; set; }
        public ETypeHistory? Type { get; set; } = ETypeHistory.Other;
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public decimal? Price{ get; set; }
    }
}