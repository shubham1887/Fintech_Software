namespace Project.Response.TransactionReport
{
    public class AmountWiseInsightsViewModel
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _AmountPercentage;

        public string AmountPercentage
        {
            get => _AmountPercentage;
            set => _AmountPercentage = value;
        }

        private decimal _ROffer;

        public decimal ROffer
        {
            get => _ROffer;
            set => _ROffer = value;
        }

        private string _ROfferPercentage;

        public string ROfferPercentage
        {
            get => _ROfferPercentage;
            set => _ROfferPercentage = value;
        }

        private int _TotalCount;

        public int TotalCount
        {
            get => _TotalCount;
            set => _TotalCount = value;
        }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private decimal _TotalAmount;

        public decimal TotalAmount
        {
            get => _TotalAmount;
            set => _TotalAmount = value;
        }

        private decimal _SuccessAmount;

        public decimal SuccessAmount
        {
            get => _SuccessAmount;
            set => _SuccessAmount = value;
        }
    }
}