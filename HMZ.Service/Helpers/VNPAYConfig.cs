using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMZ.Service.Helpers
{
    public class VNPAYConfig
    {
        public String? TmnCode { get; set; }
        public String? HashSecret { get; set; }
        public String? BaseUrl { get; set; }
        public String? Command { get; set; }
        public String? CurrCode { get; set; }
        public String? Version { get; set; }
        public String? Locale { get; set; }

        public String? ReturnUrl { get; set; }
        public String? TimeZoneId { get; set; }
    }
}