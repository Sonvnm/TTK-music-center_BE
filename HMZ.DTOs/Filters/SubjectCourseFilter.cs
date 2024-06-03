using HMZ.DTOs.Filters.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.DTOs.Filters
{
    public class SubjectCourseFilter:BaseFilter
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public Guid? CourseId { get; set; }
    }
}
