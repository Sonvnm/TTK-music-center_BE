
using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Base;
namespace HMZ.DTOs.Views
{
    public class FeedBackView : BaseView<FeedBack>
    {
        public FeedBackView(FeedBack entity) : base(entity)
        {
        }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public Guid? UserId { get; set; }
        public string? Username { get; set; }
    }
}