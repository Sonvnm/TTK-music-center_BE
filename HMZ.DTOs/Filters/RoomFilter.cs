using HMZ.DTOs.Filters.Base;
namespace HMZ.DTOs.Filters
{
    public class RoomFilter : BaseFilter
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}