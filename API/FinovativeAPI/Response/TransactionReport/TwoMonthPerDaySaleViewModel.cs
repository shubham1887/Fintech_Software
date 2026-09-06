using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class TwoMonthPerDaySaleViewModel
    {
        private string _CurrentMonth;

        public string CurrentMonth
        {
            get => _CurrentMonth.TrimIfNotNull();
            set => _CurrentMonth = value.TrimIfNotNull();
        }

        private string _LastMonth;

        public string LastMonth
        {
            get => _LastMonth.TrimIfNotNull();
            set => _LastMonth = value;
        }

        private decimal _LastEarning;

        public decimal LastEarning
        {
            get => _LastEarning;
            set => _LastEarning = value;
        }

        private decimal _CurrentEarning;

        public decimal CurrentEarning
        {
            get => _CurrentEarning;
            set => _CurrentEarning = value;
        }

        private decimal _LastGiven;

        public decimal LastGiven
        {
            get => _LastGiven;
            set => _LastGiven = value;
        }

        private decimal _CurrentGiven;

        public decimal CurrentGiven
        {
            get => _CurrentGiven;
            set => _CurrentGiven = value;
        }

        private decimal _CurrentProfit;

        public decimal CurrentProfit
        {
            get => _CurrentProfit;
            set => _CurrentProfit = value;
        }

        private decimal _LastProfit;

        public decimal LastProfit
        {
            get => _LastProfit;
            set => _LastProfit = value;
        }

        private string _ProfitInPercentage;

        public string ProfitInPercentage
        {
            get => _ProfitInPercentage.TrimIfNotNull();
            set => _ProfitInPercentage = value.TrimIfNotNull();
        }

        private int _Day;

        public int Day
        {
            get => _Day;
            set => _Day = value;
        }
    }
}