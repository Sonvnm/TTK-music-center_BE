using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
namespace HMZ.Database.Entities
{
    // Đơn hàng
    public class Order: BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; } 
        public decimal TotalPrice { get; set; } // Tổng giá trị đơn hàng
        public EOrderStatus Status { get; set; } // Trạng thái đơn hàng

        // Foreign key
        public Guid? UserId { get; set; } // Người mua
        public User? User { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }    
    }
}