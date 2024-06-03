using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMZ.DTOs.Queries
{
    public class ForgotPasswordQuery
    {
        public string? Email { get; set; }
        public string? Host { get; set; }
    }
}