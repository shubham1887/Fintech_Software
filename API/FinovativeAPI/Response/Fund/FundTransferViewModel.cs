namespace Project.Response.Fund
{
    public class FundTransferViewModel
    {
        private decimal _Transfer;

        public decimal Transfer
        {
            get => _Transfer;
            set => _Transfer = value;
        }

        private decimal _PGTransfer;

        public decimal PGTransfer
        {
            get => _PGTransfer;
            set => _PGTransfer = value;
        }

        private decimal _WalletTransfer;

        public decimal WalletTransfer
        {
            get => _WalletTransfer;
            set => _WalletTransfer = value;
        }
    }
}