using System.ComponentModel.DataAnnotations;

namespace HMZ.DTOs.Queries
{
    public class UpdatePasswordQuery
    {
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}