using HMZ.DTOs.Models;

namespace HMZ.DTOs.Queries
{
    public class StudentQuery
    {
        public string? Email { get; set; } 
        public string? Username { get; set; } 
        public string? FirstName { get; set; } 
        public string? LastName { get; set; } 
        public RangeFilter<DateTime>? DateOfBirth { get; set; }
        public string? RoleName { get; set; }
         public string? ClassCode { get; set; } // for parameter not for query
    }
}