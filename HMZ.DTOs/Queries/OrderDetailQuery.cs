using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.Database.Entities.Base;

namespace HMZ.DTOs.Queries
{
    public class OrderDetailQuery: BaseEntity
    {
        public string? CourseId { get; set; } // Khóa học
        public Guid? OrderId { get; set; } // Đơn hàng
    }
}