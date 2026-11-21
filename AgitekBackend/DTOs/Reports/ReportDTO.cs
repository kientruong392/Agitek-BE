namespace AgitekBackend.DTOs.Reports
{
    public enum ReportInterval
    {
        Day,
        Month,
        Quarter,
        Year
    }

    public class ReportQueryDTO
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public ReportInterval Interval { get; set; } = ReportInterval.Month;
    }

    public class ReportPeriodDTO
    {
        public DateTime Start { get; set; }
        public decimal Revenue { get; set; }
        public decimal Refunds { get; set; }
        public decimal NetRevenue { get; set; }
        public int StockIn { get; set; }
        public int StockOut { get; set; }
    }

    public class ProductReportDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TotalReceived { get; set; }
        public int CurrentStock { get; set; }
        public int ReceivedInPeriod { get; set; }
        public int DispatchedInPeriod { get; set; }
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ReportDTO
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public ReportInterval Interval { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal PreviousNetRevenue { get; set; }
        public decimal? GrowthPercent { get; set; }
        public int PaidOrderCount { get; set; }
        public int StockIn { get; set; }
        public int StockOut { get; set; }
        public List<ReportPeriodDTO> Periods { get; set; } = new();
        public List<ProductReportDTO> Products { get; set; } = new();
    }
}
