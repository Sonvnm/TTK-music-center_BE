using HMZ.Database.Entities.Base;
namespace HMZ.Database.Entities
{
    // Môn học
    public class Subject : BaseEntity
    {
        public string? Name { get; set; } // Tên môn học
        public string? Description { get; set; } // Mô tả
        // foreign key
        public List<Document>? Documents { get; set; } //Danh sách Tài liệu
        public List<SubjectCourse>? SubjectCourses { get; set; } // Danh sách môn học trong khóa học
    }
}