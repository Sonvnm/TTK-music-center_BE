using HMZ.Database.Commons;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMZ.Database.Entities
{
    public class Role : IdentityRole<Guid>
    {
        public String? Code { get; set; } = HMZHelper.GenerateCode(8);
        public String? CreatedBy { get; set; } = "System";
        public String? UpdatedBy { get; set; } = "System";
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public Boolean? IsActive { get; set; } = true;
        public virtual List<UserRole>? UserRoles { get; set; }
        public virtual List<RolePermission>? RolePermissions { get; set; }
    }
}
