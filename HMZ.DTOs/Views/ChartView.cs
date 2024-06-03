
namespace HMZ.DTOs.Views
{
    public class ChartView
    {

        public List<string>? Labels { get; set; }
        public List<int>? Values { get; set; }
        public List<TopCourse>? TopCourse { get; set; }
        public OrderStatistic? OrderStatistic { get; set; }
        public decimal? TotalSell { get; set; }
        public List<decimal>? PaidSalary { get; set; }
        public List<decimal>? Revenue { get; set; }
        public decimal? TotalRevenue { get; set; }
        public List<decimal>? Sells{ get; set; }

        public int? NewCourse { get; set; }
        public int? ProcessingCourse { get; set; }
        public int? EndCourse { get; set; }
        public int? Total { get; set; }
        public int? TotalToday { get; set; }
        public int? TotalYesterday { get; set; }
        public double? Percentage { get; set; }
    }

    public class TopCourse
    {
        public string? CourseName { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? TotalOrder { get; set; }
    }

    public class OrderStatistic
    {
        public int? PendingOrder { get; set; }
        public int? DoneOrder { get; set; }
        public int? FailOrder { get; set; }
    }


}