
using HMZ.DTOs.Filters.Base;

namespace HMZ.DTOs.Filters
{
    public class MessageFilter: BaseFilter
    {
        public string? Content { get; set; }
        public DateTime? SendAt { get; set; } = DateTime.Now;
        public Guid? ClassId { get; set; }
    }
}