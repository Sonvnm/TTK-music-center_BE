using HMZ.Database.Entities;
using HMZ.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.DTOs.Views
{
    public class ReviewView:BaseView<Review>
    {

        public ReviewView(Review review) :base(review) { }

        public string? Comment { get; set; } 

        public double? Rating { get; set; }

        public string? CourseName { get; set; }

        public string? UserName { get; set; }

        
    }
}
