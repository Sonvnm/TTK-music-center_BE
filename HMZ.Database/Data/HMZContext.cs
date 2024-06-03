using HMZ.Database.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HMZ.Database.Data
{

    public class HMZContext : IdentityDbContext<User, Role, Guid,
        IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {

        public HMZContext(DbContextOptions<HMZContext> options) : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlServer("Server=.;Database=HMZ_Elearning;Trusted_Connection=True;TrustServerCertificate=True");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<UserRole>()
	            .HasOne(ur => ur.User)
	            .WithMany(u => u.UserRoles)
	            .HasForeignKey(ur => ur.UserId)
	            .HasPrincipalKey(u => u.Id);

			modelBuilder.Entity<UserRole>()
				.HasOne(ur => ur.Role)
				.WithMany(ur => ur.UserRoles)
				.HasForeignKey(ur => ur.RoleId)
				.HasPrincipalKey(r => r.Id);

		}
		public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        #region Business
        public DbSet<Class> Classes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<LearningProcess> LearningProcesses { get; set; }
        public DbSet<StudentStudyProcess> StudentStudyProcesses { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<FeedBack> FeedBacks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<HistorySystem> HistorySystems { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<SubjectCourse> SubjectCourses { get; set; }
        public DbSet<Review> Review{ get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ScheduleDetail> ScheduleDetails { get; set; }

        #endregion
    }
}
