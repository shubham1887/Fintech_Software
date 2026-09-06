using Project.Processing;

namespace Project.Response.TodaySummary
{
    public class TodaySummaryViewModel
    {
        private string _Type;

        public string Type { get => _Type.TrimIfNotNull(); set => _Type = value.TrimIfNotNull(); }

        private decimal _TotalAmount;
        public decimal TotalAmount { get => _TotalAmount; set => _TotalAmount = value; }
    }
}