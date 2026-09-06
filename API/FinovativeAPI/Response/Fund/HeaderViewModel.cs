namespace Project.Response.Fund
{
    public class HeaderViewModel
    {
        private decimal _TransferAmount;

        public decimal TransferAmount
        {
            get => _TransferAmount;
            set => _TransferAmount = value;
        }

        private int _TransferCount;

        public int TransferCount
        {
            get => _TransferCount;
            set => _TransferCount = value;
        }

        private decimal _ReceiveAmount;

        public decimal ReceiveAmount
        {
            get => _ReceiveAmount;
            set => _ReceiveAmount = value;
        }

        private decimal _ReverseAmount;

        public decimal ReverseAmount
        {
            get => _ReverseAmount;
            set => _ReverseAmount = value;
        }

        private decimal _ReverseCount;

        public decimal ReverseCount
        {
            get => _ReverseCount;
            set => _ReverseCount = value;
        }

        private decimal _PGTransferAmount;

        public decimal PGTransferAmount
        {
            get => _PGTransferAmount;
            set => _PGTransferAmount = value;
        }

        private decimal _PGTransferCount;

        public decimal PGTransferCount
        {
            get => _PGTransferCount;
            set => _PGTransferCount = value;
        }

        private decimal _ECTransferAmount;

        public decimal ECTransferAmount
        {
            get => _ECTransferAmount;
            set => _ECTransferAmount = value;
        }

        private decimal _ECTransferCount;

        public decimal ECTransferCount
        {
            get => _ECTransferCount;
            set => _ECTransferCount = value;
        }

        private decimal _DownlineReverseAmount;

        public decimal DownlineReverseAmount
        {
            get => _DownlineReverseAmount;
            set => _DownlineReverseAmount = value;
        }

        private decimal _DownlineReverseCount;

        public decimal DownlineReverseCount
        {
            get => _DownlineReverseCount;
            set => _DownlineReverseCount = value;
        }

        private decimal _ReceiveCount;

        public decimal ReceiveCount
        {
            get => _ReceiveCount;
            set => _ReceiveCount = value;
        }

        private decimal _PGReceiveAmount;

        public decimal PGReceiveAmount
        {
            get => _PGReceiveAmount;
            set => _PGReceiveAmount = value;
        }

        private decimal _PGReceiveCount;

        public decimal PGReceiveCount
        {
            get => _PGReceiveCount;
            set => _PGReceiveCount = value;
        }

        private decimal _ECReceiveAmount;

        public decimal ECReceiveAmount
        {
            get => _ECReceiveAmount;
            set => _ECReceiveAmount = value;
        }

        private decimal _ECReceiveCount;

        public decimal ECReceiveCount
        {
            get => _ECReceiveCount;
            set => _ECReceiveCount = value;
        }
    }
}