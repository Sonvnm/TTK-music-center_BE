using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.DTOs.Models;

namespace HMZ.DTOs.Views
{
    public class CalculateSalaryView
    {
        public string? Username { get; set; }
        public decimal? Amount { get; set; }
        public RangeFilter<DateTime?>? CalculateDate { get; set; }
        public string? Description { get; set; }
        public int? TotalTime { get; set; }
        public int? TotalDay { get; set; }
    }
}