namespace HMZ.SDK.Excel
{
    public class ExportResult
    {
        public Dictionary<string, int>? Records { get; set; }
        public string? FileName { get; set; }
        public byte[]? Content { get; set; }
        public string? ContentType { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
