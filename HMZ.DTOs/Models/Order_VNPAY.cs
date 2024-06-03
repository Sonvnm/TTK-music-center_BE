using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMZ.DTOs.Models
{
    public class Order_VNPAY
    {
        public String? OrderType { get; set; }
        public Double? Amount { get; set; }
        public String? OrderDescription { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CourseId { get; set; }
    }
}