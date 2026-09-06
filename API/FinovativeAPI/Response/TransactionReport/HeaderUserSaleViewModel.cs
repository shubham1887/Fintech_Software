namespace Project.Response.TransactionReport
{
    public class HeaderUserSaleViewModel
    {
        private decimal _TotalSale;

        public decimal TotalSale
        {
            get => _TotalSale;
            set => _TotalSale = value;
        }

        private decimal _TotalRCommission;

        public decimal TotalRCommission
        {
            get => _TotalRCommission;
            set => _TotalRCommission = value;
        }

        private decimal _TotalDCommission;

        public decimal TotalDCommission
        {
            get => _TotalDCommission;
            set => _TotalDCommission = value;
        }

        private decimal _TotalSCommission;

        public decimal TotalSCommission
        {
            get => _TotalSCommission;
            set => _TotalSCommission = value;
        }

        private decimal _TotalPurchase;

        public decimal TotalPurchase
        {
            get => _TotalPurchase;
            set => _TotalPurchase = value;
        }

        private decimal _TotalTransfer;

        public decimal TotalTransfer
        {
            get => _TotalTransfer;
            set => _TotalTransfer = value;
        }
    }
}