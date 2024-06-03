using HMZ.Database.Entities;
using HMZ.DTOs.Base;

namespace HMZ.DTOs.Views
{
    public class SubjectView : BaseView<Subject>
    {
        public SubjectView(Subject entity) : base(entity)
        {
        }

        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}