using HMZ.Database.Enums;
using HMZ.DTOs.Filters.Base;
namespace HMZ.DTOs.Filters
{
    public class HistorySystemFilter : BaseFilter
    {
        public string? Action { get; set; }
        public string? Description { get; set; }
        public ETypeHistory? Type { get; set; }
        public string? Username { get; set; }

    }
}