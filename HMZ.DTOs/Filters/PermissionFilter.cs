using HMZ.DTOs.Filters.Base;
using HMZ.DTOs.Models;

namespace HMZ.DTOs.Filters
{
    public class PermissionFilter : BaseFilter
    {
        public String? Key { get; set; }
        public String? Value { get; set; }
        public String? Description { get; set; }

        // for role
        public String? RoleCode { get; set; }
    }
}