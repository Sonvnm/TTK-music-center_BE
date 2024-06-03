using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;

namespace HMZ.DTOs.Queries
{
    public class OrderQuery: BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; } 
        public decimal? TotalPrice { get; set; } // Tổng giá trị đơn hàng
        public string? Status { get; set; } // Trạng thái đơn hàng

        // Foreign key
        public string? UserId { get; set; } // Người mua
        public List<OrderDetailQuery>? OrderDetails { get; set; }

        public string? TransactionNo { get; set; }//Mã giao dịch
    }
}