using HMZ.Database.Entities;
using HMZ.DTOs.Base;

namespace HMZ.DTOs.Views
{
    public class CourseView : BaseView<Course>
    {
        public CourseView(Course entity) : base(entity)
        {
        }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? PublicId { get; set; }
        public string? Video { get; set; }
        public decimal Price { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? Status { get; set; }

        public string? ClassName { get; set; }
        public Guid? ClassId { get; set; }  


        public List<SubjectView> Subjects { get; set; }
    }
}