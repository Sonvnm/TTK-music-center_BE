using HMZ.DTOs.Filters.Base;
using HMZ.DTOs.Models;

namespace HMZ.DTOs.Filters
{
    public class UserFilter : BaseFilter
    {
        public String? Email { get; set; }
        public String? Username { get; set; }
        public String? FirstName { get; set; }
        public String? LastName { get; set; }
        public String? Roles { get; set; }
        public RangeFilter<DateTime?>? DateOfBirth { get; set; }
    }
}