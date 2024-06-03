using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Base;

namespace HMZ.DTOs.Views
{
    public class UserView  
    {
        public String? Email { get; set; }
        public String? Username { get; set; }
        public String? FirstName { get; set; }
        public String? LastName { get; set; }
        public String? FullName => $"{FirstName} {LastName}";
        public String? Image { get; set; }
        public String? PhoneNumber { get; set; }
        public EAccountType? AccountType { get; set; }
        public List<String>? Roles { get; set; }
        public List<RoleView>? RolesView { get; set; }
		public DateTime? DateOfBirth { get; set; }
        public string? RoleName { get; set; }

        // Default
        public Guid? Id { get; set; }
        public String? Code { get; set; }
        public String? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public String? UpdatedBy { get; set; }
        public Boolean? IsActive { get; set; }
    }
}
