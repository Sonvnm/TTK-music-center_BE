using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.DTOs.Models;
using Microsoft.AspNetCore.Http;

namespace HMZ.DTOs.Queries
{
    public class CalculateSalaryQuery
    {
        public string? Username { get; set; }
        public decimal? Amount { get; set; }
        public RangeFilter<DateTime?>? CalculateDate { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Description { get; set; }
        public IFormFile? File{ get; set; }
    }

 
}