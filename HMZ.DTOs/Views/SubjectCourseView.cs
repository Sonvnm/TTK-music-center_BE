using HMZ.Database.Entities;
using HMZ.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.DTOs.Views
{
    public class SubjectCourseView : BaseView<SubjectCourse>
    {

        public SubjectCourseView(SubjectCourse entity) : base(entity)
        {
            

        }
        public SubjectCourseView() : base()
        {

        }
        public string? CourseCode { get; set; }
        public Guid? CourseId { get; set; }
        public Course Course { get; set; }
        public List<Subject> Subjects { get; set; }

        public List<Class> Classes{ get; set; }

        //Check if user already bought course
        public bool? IsBought { get; set; }

        public bool? RatingAvailable { get; set; }



    }
}
