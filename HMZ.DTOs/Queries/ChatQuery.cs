
namespace HMZ.DTOs.Queries
{
    public class ChatQuery
    {
        public Guid? ClassId { get; set; }
        public Guid? UserId { get; set; }
        public string? Username { get; set; }
        public string? Avatar { get; set; }
        public string? Content { get; set; }
    }
}