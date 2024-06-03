using System.ComponentModel.DataAnnotations;

namespace HMZ.DTOs.Queries
{
    public class RegisterQuery
    {
        public String? FirstName { get; set; }
        public String? LastName { get; set; }
        public String? Email { get; set; }
        public String? Password { get; set; }
        public String? ConfirmPassword { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
