using HMZ.Database.Enums;
using HMZ.DTOs.Filters.Base;
using HMZ.DTOs.Models;

namespace HMZ.DTOs.Filters
{
    public class OrderFilter: BaseFilter
    {
        public string? Name { get; set; }
        public string? Description { get; set; } 
        public RangeFilter<decimal?>? TotalPrice { get; set; } // Tổng giá trị đơn hàng
        public EOrderStatus? Status { get; set; } = null; // Trạng thái đơn hàng
        public string? Username {get;set;} // Nguoi mua hang
    }
}