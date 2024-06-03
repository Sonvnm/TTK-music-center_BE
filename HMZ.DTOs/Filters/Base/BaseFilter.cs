using HMZ.DTOs.Models;

namespace HMZ.DTOs.Filters.Base
{
    public class BaseFilter
    {
        public String? Code { get; set; } 
        public String? CreatedBy { get; set; } 
        public String? UpdatedBy { get; set; } 
        public RangeFilter<DateTime?>? CreatedAt { get; set; } 
        public RangeFilter<DateTime?>? UpdatedAt { get; set; }
        public Boolean? IsActive { get; set; }  = true;
    }
}