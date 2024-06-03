

using HMZ.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMZ.Database.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.Property(x => x.Name).HasMaxLength(40);
            builder.Property(x => x.Description).HasMaxLength(250);

            builder.HasOne(x => x.Course).WithMany(x => x.Classes).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
      
            #endregion Custom
        }
    }
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.Property(x => x.Name).HasMaxLength(40);
            builder.Property(x => x.Description).HasMaxLength(250);

            builder.HasOne(x => x.Class).WithMany(x => x.Documents).HasForeignKey(x => x.ClassId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.User).WithMany(x => x.Documents).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }

    public class FeedBackConfiguration : IEntityTypeConfiguration<FeedBack>
    {
        public void Configure(EntityTypeBuilder<FeedBack> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.Property(x => x.Title).HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(250);

            builder.HasOne(x => x.User).WithMany(x => x.FeedBacks).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }

     public class HistorySystemConfiguration : IEntityTypeConfiguration<HistorySystem>
    {
        public void Configure(EntityTypeBuilder<HistorySystem> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.Property(x => x.Action).HasMaxLength(40);
            builder.Property(x => x.Description).HasMaxLength(250);

            builder.HasOne(x => x.User).WithMany(x => x.HistorySystems).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }
    public class LearningProcessConfiguration : IEntityTypeConfiguration<LearningProcess>
    {
        public void Configure(EntityTypeBuilder<LearningProcess> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.Property(x => x.Name).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(250);

            builder.HasOne(x => x.User).WithMany(x => x.LearningProcesses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ScheduleDetail).WithMany(x => x.LearningProcesses).HasForeignKey(x => x.ScheduleDetailId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }
    public class OrderProcessConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.Property(x => x.Name).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(250);

            builder.HasOne(x => x.User).WithMany(x => x.Orders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }
    public class OrderDetailProcessConfiguration : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 

            builder.HasOne(x => x.Course).WithMany(x => x.OrderDetails).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Order).WithMany(x => x.OrderDetails).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }

    public class RoomProcessConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.Property(x => x.Name).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(250);
            #endregion Custom
        }
    }

    public class ScheduleProcessConfiguration : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.Property(x => x.Name).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(250);

            builder.HasOne(x => x.Course).WithMany(x => x.Schedules).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }

    public class ScheduleDetailConfiguration : IEntityTypeConfiguration<ScheduleDetail>
    {

        public void Configure(EntityTypeBuilder<ScheduleDetail> builder)
        {
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.HasOne(x => x.Schedule).WithMany(x => x.ScheduleDetails).HasForeignKey(x => x.ScheduleId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }

    public class StudentClassProcessConfiguration : IEntityTypeConfiguration<StudentClass>
    {
        public void Configure(EntityTypeBuilder<StudentClass> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 

            builder.HasOne(x => x.User).WithMany(x => x.StudentClasses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Class).WithMany(x => x.StudentClasses).HasForeignKey(x => x.ClassId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }
    public class StudentStudyProcessConfiguration : IEntityTypeConfiguration<StudentStudyProcess>
    {
        public void Configure(EntityTypeBuilder<StudentStudyProcess> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 

            builder.HasOne(x => x.User).WithMany(x => x.StudentStudyProcesses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LearningProcess).WithMany(x => x.StudentStudyProcesses).HasForeignKey(x => x.LearningProcessId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }
    public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.Property(x => x.Name).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(250);
            #endregion Custom
        }
    }
     public class SubjectCourseConfiguration : IEntityTypeConfiguration<SubjectCourse>
    {
        public void Configure(EntityTypeBuilder<SubjectCourse> builder)
        {
            // default
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.HasOne(x => x.Subject).WithMany(x => x.SubjectCourses).HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Course).WithMany(x => x.SubjectCourses).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }     
    
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {

        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.HasOne(x => x.User).WithMany(x => x.Review).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Course).WithMany(x => x.Review).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }

    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {

        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(x => x.Id);

            #region Custom 
            builder.HasOne(x => x.User).WithMany(x => x.Messages).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Class).WithMany(x => x.Messages).HasForeignKey(x => x.ClassId).OnDelete(DeleteBehavior.Restrict);
            #endregion Custom
        }
    }
}