using System.ComponentModel;

namespace HMZ.DTOs.Queries.Base
{
    public class BaseQuery<T>
    {
        [DefaultValue(1)]
        public Int32? PageNumber { get; set; } = 1;

        [DefaultValue(10)]
        public Int32? PageSize { get; set; } = 10;
        public T? Entity { get; set; }
        public List<String>? SortColumns { get; set; }
        public bool? SortOrder { get; set; } = true; // true: DESC, false: ASC

    }
}

