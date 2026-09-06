namespace Project.Response.Fund
{
    public class DashboardFundViewModel
    {
        private int _TransferCount;

        public int TransferCount
        {
            get => _TransferCount;
            set => _TransferCount = value;
        }

        private decimal _TransferAmount;

        public decimal TransferAmount
        {
            get => _TransferAmount;
            set => _TransferAmount = value;
        }

        private decimal _PGTransferAmount;

        public decimal PGTransferAmount
        {
            get => _PGTransferAmount;
            set => _PGTransferAmount = value;
        }

        private decimal _ReverseAmount;

        public decimal ReverseAmount
        {
            get => _ReverseAmount;
            set => _ReverseAmount = value;
        }

        private decimal _PurchaseAmount;

        public decimal PurchaseAmount
        {
            get => _PurchaseAmount;
            set => _PurchaseAmount = value;
        }

        private int _PurchaseCount;

        public int PurchaseCount
        {
            get => _PurchaseCount;
            set => _PurchaseCount = value;
        }

        private decimal _DownlineReverseAmount;

        public decimal DownlineReverseAmount
        {
            get => _DownlineReverseAmount;
            set => _DownlineReverseAmount = value;
        }

        private decimal _PGPurchaseAmount;

        public decimal PGPurchaseAmount
        {
            get => _PGPurchaseAmount;
            set => _PGPurchaseAmount = value;
        }

        private decimal _ECollectionAmount;

        public decimal ECollectionAmount
        {
            get => _ECollectionAmount;
            set => _ECollectionAmount = value;
        }
    }
}