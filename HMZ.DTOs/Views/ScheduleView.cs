using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.DTOs.Views
{
    public class ScheduleView : BaseView<Schedule>
    {
        public ScheduleView(Schedule entity) : base(entity)
        {
        }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? CourseId { get; set; }
        public Course? Course { get; set; }

        public Guid? ClassId { get; set; }
        public string? ClassName{ get; set; }
        public Class? Class { get; set; }
        public string Status { get; set; }


    }
}
