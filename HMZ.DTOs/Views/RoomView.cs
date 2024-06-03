
using HMZ.Database.Entities;
using HMZ.DTOs.Base;
namespace HMZ.DTOs.Views
{
    public class RoomView : BaseView<Room>
    {
        public RoomView(Room entity) : base(entity)
        {
        }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}