using HMZ.Database.Entities.Base;

namespace HMZ.DTOs.Queries
{
    public class StudentClassQuery: BaseEntity
    {
        public string? ClassCode { get; set; }
        public List<string>? UserCode { get; set; }
    }
}