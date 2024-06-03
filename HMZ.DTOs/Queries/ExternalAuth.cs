using System.ComponentModel.DataAnnotations;

namespace HMZ.DTOs.Queries
{
    public class ExternalAuth
    {
        [Required]
        public string? Provider { get; set; }
        [Required]
        public string? IdToken { get; set; }
    }
}