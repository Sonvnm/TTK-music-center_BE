using HMZ.Database.Entities.Base;
namespace HMZ.Database.Entities
{
    public class SubjectCourse : BaseEntity
    {
        public Guid? SubjectId { get; set; }
        public Guid? CourseId { get; set; }
        public virtual Subject? Subject { get; set; }
        public virtual Course? Course { get; set; }
        

        public SubjectCourse(Guid? subjectId, Guid? courseId, Subject? subject, Course? course)
        {
            SubjectId = subjectId;
            CourseId = courseId;
            Subject = subject;
            Course = course;
        }

        public SubjectCourse()
        {
        }

        public SubjectCourse CreateSubjectCourse(Subject subject,Course course)
        {
            return new SubjectCourse
            {
                SubjectId = subject.Id,
                CourseId = course.Id, 
                Subject = subject,
                Course = course

            };
        }
    }
}