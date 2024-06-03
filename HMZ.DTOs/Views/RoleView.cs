namespace HMZ.DTOs.Views
{
    public class RoleView
    {
        public String? Id { get; set; }
        public String? Name { get; set; }

        public List<PermissionView>? Permissions { get; set; }

        public String? Code { get; set; } 
        public String? CreatedBy { get; set; } 
        public String? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; } 
        public Boolean? IsActive { get; set; }
    }
}