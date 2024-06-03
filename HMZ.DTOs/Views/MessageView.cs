using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.Database.Entities;
using HMZ.DTOs.Base;

namespace HMZ.DTOs.Views
{
    public class MessageView : BaseView<Message>
    {
        public MessageView(Message entity) : base(entity)
        {
        }

        public string? Content { get; set; }
        public DateTime? SendAt { get; set; } = DateTime.Now;
        public Guid? UserId { get; set; }
        public UserView? User { get; set; }
        public Guid? ClassId { get; set; }
        public Class? Class { get; set; }
        public string? TimeAgo { get; set; }
    }
}