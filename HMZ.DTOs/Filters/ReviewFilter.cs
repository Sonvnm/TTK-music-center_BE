using HMZ.DTOs.Filters.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.DTOs.Filters
{
    public class ReviewFilter:BaseFilter
    {
        public double? Rating {  get; set; } 

        public string? Comment { get; set; }
        public string? UserName { get; set; }
        public string? CourseName { get; set; }


    }
}
