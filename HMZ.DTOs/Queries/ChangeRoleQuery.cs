namespace HMZ.DTOs.Queries
{
    public class ChangeRoleQuery
    {
        public string? UserId { get; set; }
        public List<string>? Roles { get; set; }
    }
}