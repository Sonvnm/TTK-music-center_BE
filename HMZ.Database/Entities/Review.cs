using HMZ.Database.Entities.Base;

namespace HMZ.Database.Entities
{
    public class Review:BaseEntity

    {
        public string? Comment { get; set; }

        public double? Rating { get; set; }
        public Guid? CourseId { get; set; }

        public Guid? UserId { get; set; }

        public User? User { get; set; }

        public Course? Course { get; set; }
    }
}
