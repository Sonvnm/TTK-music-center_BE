using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.Database.Entities;
using HMZ.DTOs.Base;

namespace HMZ.DTOs.Views
{
    public class ScheduleDetailView : BaseView<ScheduleDetail>
    {
        public ScheduleDetailView(ScheduleDetail entity) : base(entity)
        {
        }
        public string? Name { get; set; }
        public Guid? ScheduleId { get; set; } // Lịch học
        public string? ScheduleName { get; set; } // Tên buổi học
        public Guid? RoomId { get; set; } // Phòng học
        public string? RoomName { get; set; } // Tên phòng học
        public string? ClassName { get; set; }
        public Guid? ClassId { get; set; } 
        public DateTime? StartTime { get; set; } // Thời gian bắt đầu
        public DateTime? EndTime { get; set; } // Thời gian kết thúc
        public bool? IsMakeUpClass{ get; set; } // Thời gian kết thúc
    }
}