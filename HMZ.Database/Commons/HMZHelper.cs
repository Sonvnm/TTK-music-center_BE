namespace HMZ.Database.Commons
{
    public static class HMZHelper
    {

        public static string GenerateCode(int length, string prefix = "")
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[length];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                if (i == 0)
                {
                    stringChars[i] = chars[random.Next(10, chars.Length)];
                }
                else
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }
            }
            return prefix + new String(stringChars);
        }

        public static string TimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            double totalSeconds = timeSpan.TotalSeconds;
            double totalMinutes = timeSpan.TotalMinutes;
            double totalHours = timeSpan.TotalHours;
            double totalDays = timeSpan.TotalDays;

            if (totalSeconds <= 60)
            {
                return $"{totalSeconds:0} giây trước";
            }
            else if (totalMinutes <= 60)
            {
                return totalMinutes > 1 ? $"khoảng {totalMinutes:0} phút trước" : "khoảng một phút trước";
            }
            else if (totalHours <= 24)
            {
                return totalHours > 1 ? $"khoảng {totalHours:0} giờ trước" : "khoảng một giờ trước";
            }
            else if (totalDays <= 30)
            {
                return totalDays > 1 ? $"khoảng {totalDays:0} ngày trước" : "hôm qua";
            }
            else if (totalDays <= 365)
            {
                double months = totalDays / 30;
                return months > 1 ? $"khoảng {months:0} tháng trước" : "khoảng một tháng trước";
            }
            else
            {
                double years = totalDays / 365;
                return years > 1 ? $"khoảng {years:0} năm trước" : "khoảng một năm trước";
            }
        }

    }
}
