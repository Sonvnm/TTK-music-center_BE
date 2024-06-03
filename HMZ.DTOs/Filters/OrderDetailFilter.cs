
using HMZ.DTOs.Filters.Base;
namespace HMZ.DTOs.Filters
{
    public class OrderDetailFilter: BaseFilter
    {
        public Guid? CourseId { get; set; } // Khóa học
        public Guid? OrderId { get; set; } // Đơn hàng
        public string? CourseCode { get; set; } // Mã khóa học
        public string? CourseName { get; set; } // Tên khóa học
        public decimal? CoursePrice { get; set; } // Giá khóa học
    }
}