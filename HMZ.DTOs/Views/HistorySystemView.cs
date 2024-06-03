using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Base;
namespace HMZ.DTOs.Views
{
    public class HistorySystemView : BaseView<HistorySystem>
    {
        public HistorySystemView(HistorySystem entity) : base(entity)
        {
        }

        public string? Action { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Username { get; set; }
        public decimal? Price { get; set; }
    }
}