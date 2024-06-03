namespace HMZ.DTOs.Queries.Base
{
    public class BasePropertyQuery
    {
        public Guid? Id { get; set; } 
        public String? Code { get; set; }
        public String? CreatedBy { get; set; } 
        public String? UpdatedBy { get; set; } 
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Boolean? IsActive { get; set; }
    }
}