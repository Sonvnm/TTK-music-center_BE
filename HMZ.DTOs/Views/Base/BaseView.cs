using HMZ.Database.Entities.Base;
namespace HMZ.DTOs.Base
{
    public class BaseView<T>  where T : BaseEntity
    {
        public Guid? Id { get; set; } 
        public String? Code { get; set; }
        public String? CreatedBy { get; set; } 
        public String? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Boolean? IsActive { get; set; }
        public BaseView(T entity)
        {
            Id = entity.Id;
            Code = entity.Code;
            CreatedBy = entity.CreatedBy;
            CreatedAt = entity.CreatedAt;
            UpdatedAt = entity.UpdatedAt;
            IsActive = entity.IsActive;
            UpdatedBy = entity.UpdatedBy;
        }

        public BaseView()
        {
        }
    }
}
