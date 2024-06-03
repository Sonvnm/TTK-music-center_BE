
using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
namespace HMZ.Database.Entities
{
    // Phản hồi tổng của người dùng
    public class FeedBack: BaseEntity
    {
        public string? Title { get; set; } // Tiêu đề
        public string? Description { get; set; } // Mô tả
        public ETypeFeedBack Type { get; set; } = ETypeFeedBack.Other; // Loại phản hồi
        public EFeedBackStatus Status { get; set; } = EFeedBackStatus.New; // Trạng thái phản hồi

        // Foreign key
        public Guid? UserId { get; set; } // Người phản hồi
        public User? User { get; set; }
    }
}