using HMZ.Database.Entities.Base;
namespace HMZ.DTOs.Queries
{
    public class UserQuery : BaseEntity
    {
        public String? Email { get; set; } 
        public String? Username { get; set; } 
        public String? Password { get; set; }  
        public String? FirstName { get; set; } 
        public String? LastName { get; set; } 
        public string? PhoneNumber{ get; set; } 
        public String[]? Roles { get; set; } 
        public string? Image { get; set; }
        public string? PublicId { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
