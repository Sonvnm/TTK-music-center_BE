
using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
namespace HMZ.DTOs.Queries
{
    public class FeedBackQuery : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ETypeFeedBack Type { get; set; }
        public EFeedBackStatus Status { get; set; }
        public string? Username { get; set; }
        public Guid? UserId { get; set; }
    }
}