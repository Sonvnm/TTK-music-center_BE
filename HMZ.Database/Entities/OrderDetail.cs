using HMZ.Database.Entities.Base;
namespace HMZ.Database.Entities
{
    // Chi tiết đơn hàng
    public class OrderDetail : BaseEntity
    {
        public Guid? CourseId { get; set; } // Khóa học
        public Course? Course { get; set; }
        public Guid? OrderId { get; set; } // Đơn hàng
        public Order? Order { get; set; }

    }
}