using HMZ.Database.Enums;
using HMZ.DTOs.Filters.Base;

namespace HMZ.DTOs.Filters
{
    public class FeedBackFilter : BaseFilter
    {
        public string? Title { get; set; } 
        public string? Description { get; set; }
        public ETypeFeedBack? Type { get; set; }  
        public EFeedBackStatus? Status { get; set; }  
        public string? Username { get; set; }
    }
}