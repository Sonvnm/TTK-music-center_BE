
using HMZ.Database.Entities;
using HMZ.DTOs.Base;

namespace HMZ.DTOs.Views
{
    public class OrderDetailView: BaseView<OrderDetail>
    {
        public OrderDetailView(OrderDetail entity) : base(entity)
        {
        }

        public Guid? CourseId { get; set; } // Khóa học
        public Guid? OrderId { get; set; } // Đơn hàng
        public string? CourseCode { get; set; } // Mã khóa học
        public string? CourseName { get; set; } // Tên khóa học
        public decimal? CoursePrice { get; set; } // Giá khóa học
        public string? CourseImage { get; set; } // Hình khóa học
        public DateTime? CourseStartDate{ get; set; } 
        public DateTime? CourseEndDate { get; set; } 
        public int? SubjectsLength{ get; set; } // Số môn học
    }
}