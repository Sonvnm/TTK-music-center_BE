using System.ComponentModel.DataAnnotations;
using HMZ.Database.Commons;

namespace HMZ.Database.Entities.Base
{
    public class BaseEntity
    {
        [Key]
        public Guid? Id { get; set; } = Guid.NewGuid();
        public String? Code { get; set; } = HMZHelper.GenerateCode(16);
        public String? CreatedBy { get; set; } = "System";
        public String? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public Boolean? IsActive { get; set; } = true;
    }
}
