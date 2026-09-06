using Project.Processing;

namespace Project.Response.SaleGrowth
{
    public class SaleGrowthViewModel
    {
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _LastWeekCount;

        public int LastWeekCount
        {
            get => _LastWeekCount;
            set => _LastWeekCount = value;
        }

        private decimal _LastWeekAmt;

        public decimal LastWeekAmt
        {
            get => _LastWeekAmt;
            set => _LastWeekAmt = value;
        }

        private int _LastofLastWeekCount;

        public int LastofLastWeekCount
        {
            get => _LastofLastWeekCount;
            set => _LastofLastWeekCount = value;
        }

        private decimal _LastofLastWeekAmt;

        public decimal LastofLastWeekAmt
        {
            get => _LastofLastWeekAmt;
            set => _LastofLastWeekAmt = value;
        }
    }
}