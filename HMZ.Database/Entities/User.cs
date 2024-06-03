using HMZ.Database.Commons;
using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMZ.Database.Entities
{
    public class User : IdentityUser<Guid>
    {
        public String? FirstName { get; set; }
        public String? LastName { get; set; }
        public string? Image { get; set; }
        public string? PublicId { get; set; } // PublicId của ảnh đại diện - Cloudinary
        public EAccountType? AccountType { get; set; } = EAccountType.Email;
        public DateTime? DateOfBirth { get; set; }

        // Foreign key
        public virtual List<UserRole>? UserRoles { get; set; }
        public virtual List<Document>? Documents { get; set; }
        public virtual List<FeedBack>? FeedBacks { get; set; }
        public virtual List<HistorySystem>? HistorySystems { get; set; }
        public virtual List<LearningProcess>? LearningProcesses { get; set; }
        public virtual List<Order>? Orders { get; set; }
        public virtual List<StudentClass>? StudentClasses { get; set; }
        public virtual List<StudentStudyProcess>? StudentStudyProcesses { get; set; }
        public virtual List<Review>? Review { get; set; }
        public virtual List<Message>? Messages { get; set; }
        // Base
        public String? Code { get; set; } = HMZHelper.GenerateCode(8);
        public Boolean? IsActive { get; set; } = true;
        public String? CreatedBy { get; set; } = "System";
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public String? UpdatedBy { get; set; }  = "System";
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
