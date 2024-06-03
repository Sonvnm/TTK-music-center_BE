using HMZ.Database.Entities.Base;
using Microsoft.AspNetCore.Http;
namespace HMZ.DTOs.Queries
{
    public class CourseQuery : BaseEntity
    {
        public string? Name { get; set; } 
        public string? Description { get; set; } 
        public string? Image { get; set; } 
        public string? Video { get; set; } 
        public decimal Price { get; set; } 
        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; } 
        public bool? Status { get; set; } = false;

        public string? PublicId { get; set; }
    }
}