
using HMZ.Database.Entities;
using HMZ.DTOs.Base;
namespace HMZ.DTOs.Views
{
    public class ClassView : BaseView<Class>
    {
      
        public ClassView(Class entity) : base(entity)
        {
        }

     
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CourseName { get; set; }
        public Guid? CourseId { get; set; }
        public CourseView? Course { get; set; }
        public List<StudentClass>? Students { get; set; }
        public List<StudentClass>? Teachers { get; set; }

        public List<StudentClass> StudentClasses { get; set; }  

        public List<Room>? Rooms { get; set;}
    }
}