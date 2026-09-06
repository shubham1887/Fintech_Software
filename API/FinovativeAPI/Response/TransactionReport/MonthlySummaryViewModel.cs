namespace Project.Response.TransactionReport
{
    public class MonthlySummaryViewModel
    {
        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _Earning;

        public decimal Earning
        {
            get => _Earning;
            set => _Earning = value;
        }

        private decimal _Given;

        public decimal Given
        {
            get => _Given;
            set => _Given = value;
        }

        private decimal _Profit;

        public decimal Profit
        {
            get => _Profit;
            set => _Profit = value;
        }

        private int _Day;

        public int Day
        {
            get => _Day;
            set => _Day = value;
        }
    }
}