using HMZ.Database.Entities.Base;

namespace HMZ.Database.Entities
{
    public class Permission : BaseEntity
    {
        public String? Key { get; set; }
        public String? Value { get; set; }
        public String? Description { get; set; }

        // Foreign key
        public virtual List<RolePermission>? RolePermissions { get; set; }
    }
}
