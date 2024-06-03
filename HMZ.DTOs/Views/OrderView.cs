using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Base;

namespace HMZ.DTOs.Views
{
    public class OrderView : BaseView<Order>
    {
        public OrderView(Order entity) : base(entity)
        {
        }

        public string? Name { get; set; }
        public string? Description { get; set; } 
        public decimal? TotalPrice { get; set; } // Tổng giá trị đơn hàng
        public string? Status { get; set; } // Trạng thái đơn hàng
        public string? CourseName { get; set; } // Trạng thái đơn hàng

        // Foreign key
        public Guid? UserId { get; set; } 
        public string? Username { get; set; } // Người mua
        public List<OrderDetailView>? OrderDetails { get; set; }
    }
}