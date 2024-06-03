namespace HMZ.DTOs.Models
{
    public class RangeFilter<T>
    {
        public T? FromValue { get; set; }
        public T? ToValue { get; set; }
    }
}
