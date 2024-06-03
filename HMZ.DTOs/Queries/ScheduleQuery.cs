using HMZ.Database.Entities;
using HMZ.Database.Entities.Base;
using HMZ.Database.Enums;
using HMZ.DTOs.Queries.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.DTOs.Queries
{
    public class ScheduleQuery:BaseEntity
    {

         public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? ClassId { get; set; }
        public EScheduleStatus Status { get; set; }


    }
}
