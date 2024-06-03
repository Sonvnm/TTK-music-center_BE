using HMZ.Database.Entities;
using HMZ.DTOs.Base;
namespace HMZ.DTOs.Views
{
    public class PermissionView : BaseView<Permission>
    {
        public Guid? RoleId { get; set; }
        public String? RoleName { get; set; }
        public String? RoleCode { get; set; }
        public Guid? PermissionId { get; set; }
        public String? Key { get; set; }
        public String? Value { get; set; }
        public String? Description { get; set; }
        public PermissionView(Permission entity) : base(entity)
        {
            
        }
    }
}
