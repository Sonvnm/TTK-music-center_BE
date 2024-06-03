namespace HMZ.DTOs.Views
{
    public class ChatView
    {
        public Guid? ClassId { get; set; }
        public string? ClassName { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Role { get; set; }
        public string? Content { get; set; }
        public DateTime? SendAt { get; set; }
        public string? TimeAgo { get; set; }
        public UserView? User { get; set; }
    }
}