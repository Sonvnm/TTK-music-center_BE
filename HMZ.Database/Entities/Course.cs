using HMZ.Database.Entities.Base;
namespace HMZ.Database.Entities
{
    // Khóa học
    public class Course : BaseEntity
    {
        public string? Name { get; set; } // Tên khóa học
        public string? Description { get; set; } // Mô tả
        public string? Image { get; set; } // Ảnh đại diện
        public string? Video { get; set; } // Video giới thiệu
        public decimal Price { get; set; } // Giá
        public DateTime? StartDate { get; set; }  // Ngày bắt đầu
        public DateTime? EndDate { get; set; } // Ngày kết thúc
        public bool? Status { get; set; } = true; // false: Đang đóng, true: Đang mở
        public string? PublicId { get; set; } // PublicId của ảnh đại diện

        // Foreign key
        public List<Class>? Classes { get; set; } // Danh sách lớp học
        public List<SubjectCourse>? SubjectCourses { get; set; } // Danh sách môn học trong khóa học
        public List<OrderDetail>? OrderDetails { get; set; } // Danh sách đơn hàng
        public List<Schedule>? Schedules { get; set; } // Danh sách lịch học

        public List<Review>? Review { get; set; }

    }
}
