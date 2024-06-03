
using HMZ.Database.Entities.Base;
namespace HMZ.DTOs.Queries
{
    public class RoomQuery : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}