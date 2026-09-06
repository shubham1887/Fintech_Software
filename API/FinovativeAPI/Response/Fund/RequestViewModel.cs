using Project.Processing;

namespace Project.Response.Fund
{
    public class RequestViewModel
    {
        private string dOC;

        public string DOC
        {
            get => dOC;
            set => dOC = value;
        }

        private string sender;

        public string Sender
        {
            get => sender.TrimIfNotNull();
            set => sender = value.TrimIfNotNull();
        }

        private string receiver;

        public string Receiver
        {
            get => receiver.TrimIfNotNull();
            set => receiver = value.TrimIfNotNull();
        }

        private string _OrignalFirmName;

        public string OrignalFirmName
        {
            get => _OrignalFirmName.TrimIfNotNull();
            set => _OrignalFirmName = value.TrimIfNotNull();
        }

        private string walletName;

        public string Wallet
        {
            get => walletName.TrimIfNotNull();
            set => walletName = value.TrimIfNotNull();
        }

        private string payMode;

        public string PayMode
        {
            get => payMode;
            set => payMode = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private decimal _OriginalAmount;

        public decimal OriginalAmount
        {
            get => _OriginalAmount;
            set => _OriginalAmount = value;
        }

        private decimal amount;

        public decimal Amount
        {
            get => amount;
            set => amount = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _SenderRemark;

        public string SenderRemark
        {
            get => _SenderRemark.TrimIfNotNull();
            set => _SenderRemark = value.TrimIfNotNull();
        }

        private string remark;

        public string Remark
        {
            get => remark.TrimIfNotNull();
            set => remark = value.TrimIfNotNull();
        }

        private decimal _SenderOpeningBalance;

        public decimal SenderOpeningBalance
        {
            get => _SenderOpeningBalance;
            set => _SenderOpeningBalance = value;
        }

        private decimal _SenderClosingBalance;

        public decimal SenderClosingBalance
        {
            get => _SenderClosingBalance;
            set => _SenderClosingBalance = value;
        }

        private decimal receiverOpeningBalance;

        public decimal ReceiverOpeningBalance
        {
            get => receiverOpeningBalance;
            set => receiverOpeningBalance = value;
        }

        private decimal receiverClosingBalance;

        public decimal ReceiverClosingBalance
        {
            get => receiverClosingBalance;
            set => receiverClosingBalance = value;
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _UTRNumber;

        public string UTRNumber
        {
            get => _UTRNumber.TrimIfNotNull();
            set => _UTRNumber = value.TrimIfNotNull();
        }

        private int _FundTransactionID;

        public int FundTransactionID
        {
            get => _FundTransactionID;
            set => _FundTransactionID = value;
        }

        private string _TransactionDOC;

        public string TransactionDOC
        {
            get => _TransactionDOC;
            set => _TransactionDOC = value;
        }

        private string _UpdatedDOC;

        public string UpdatedDOC
        {
            get => _UpdatedDOC;
            set => _UpdatedDOC = value;
        }

        private string _Through;

        public string Through
        {
            get => _Through.TrimIfNotNull();
            set => _Through = value.TrimIfNotNull();
        }

        private string dOCType;

        internal string DOCType
        {
            get => dOCType.TrimIfNotNull();
            set => dOCType = value.TrimIfNotNull();
        }

        private string _ReceiptPath;

        public string ReceiptPath
        {
            get => _ReceiptPath.TrimIfNotNull();
            set => _ReceiptPath = value.TrimIfNotNull();
        }

        private string _AdminRemark;

        public string AdminRemark
        {
            get => _AdminRemark;
            set => _AdminRemark = value;
        }

        private int _ReceiverID;

        public int ReceiverID
        {
            get => _ReceiverID;
            set => _ReceiverID = value;
        }

        private decimal _SenderBalance;

        public decimal SenderBalance
        {
            get => _SenderBalance;
            set => _SenderBalance = value;
        }

        private int _SenderID;

        public int SenderID
        {
            get => _SenderID;
            set => _SenderID = value;
        }

        private string _ReceiverName;

        public string ReceiverName
        {
            get => _ReceiverName.TrimIfNotNull();
            set => _ReceiverName = value.TrimIfNotNull();
        }

        private string _ReceiverFirmName;

        public string ReceiverFirmName
        {
            get => _ReceiverFirmName.TrimIfNotNull();
            set => _ReceiverFirmName = value.TrimIfNotNull();
        }

        private string _ReceiverMobile;

        public string ReceiverMobile
        {
            get => _ReceiverMobile.TrimIfNotNull();
            set => _ReceiverMobile = value.TrimIfNotNull();
        }

        private string _SenderName;

        public string SenderName
        {
            get => _SenderName.TrimIfNotNull();
            set => _SenderName = value.TrimIfNotNull();
        }

        private string _SenderFirmName;

        public string SenderFirmName
        {
            get => _SenderFirmName.TrimIfNotNull();
            set => _SenderFirmName = value.TrimIfNotNull();
        }

        private string _SenderMobile;

        public string SenderMobile
        {
            get => _SenderMobile.TrimIfNotNull();
            set => _SenderMobile = value.TrimIfNotNull();
        }

        private int _TxnID;

        public int TxnID
        {
            get => _TxnID;
            set => _TxnID = value;
        }

        private string _RequestAcceptedUser;

        public string RequestAcceptedUser
        {
            get => _RequestAcceptedUser.TrimIfNotNull();
            set => _RequestAcceptedUser = value.TrimIfNotNull();
        }

        private string _ParentName;

        public string ParentName
        {
            get => _ParentName.TrimIfNotNull();
            set => _ParentName = value.TrimIfNotNull();
        }

        private bool _IsDelete;

        public bool IsDelete
        {
            get => _IsDelete;
            set => _IsDelete = value;
        }
        public int WalletID { get; internal set; }


        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

    }
}