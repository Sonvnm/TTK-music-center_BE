using HMZ.Database.Entities.Base;
using HMZ.DTOs.Queries.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.DTOs.Queries
{
    public class ReviewQuery:BaseEntity
    {
        public string Comment { get; set; }
        public Guid UserId {  get; set; }
        public Guid CourseId { get; set; }

        public double Rating { get; set; }
    }
}
