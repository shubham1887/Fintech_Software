using Project.Processing;

namespace Project.Response.Ledger
{
    public class LedgerResponse
    {
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _WalletName;

        public string WalletName
        {
            get => _WalletName;
            set => _WalletName = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }
        
        private decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get { return _OpeningBalance; }
            set { _OpeningBalance = value; }
        }
        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }
        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
        
        public CustomAmount customAmount = new CustomAmount();

        private decimal _CommissionAmount;

        public decimal CommissionAmount
        {
            get { return _CommissionAmount; }
            set { _CommissionAmount = value; }
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get { return _ClosingBalance; }
            set { _ClosingBalance = value; }
        }

        private string _AssignedUserName;

        public string AssignedUserName
        {
            get => _AssignedUserName;
            set => _AssignedUserName = value;
        }

        private string _ReferenceID;

        public string ReferenceID
        {
            get => _ReferenceID.TrimIfNotNull();
            set => _ReferenceID = value.TrimIfNotNull();
        } 
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }
     
        private int _TransactionDetailID;

        public int TransactionDetailID
        {
            get => _TransactionDetailID;
            set => _TransactionDetailID = value;
        }

        private int _UserID;

        public int UserID
        {
            get { return _UserID; }
            set { _UserID = value; }
        }

        private string _AmountCreditDebit;

        public string AmountCreditDebit
        {
            get => _AmountCreditDebit.TrimIfNotNull();
            set => _AmountCreditDebit = value.TrimIfNotNull();
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _DOCType;

        public string DOCType
        {
            get => _DOCType;
            set => _DOCType = value;
        }
        private decimal _TDS;

        public decimal TDS
        {
            get => _TDS;
            set => _TDS = value;
        }
        private string _RechargeDate;

        public string RechargeDate
        {
            get => _RechargeDate;
            set => _RechargeDate = value;
        }
    }
    
    public class CustomAmount
    {
        private decimal _Amount;

        public decimal Amount { get => _Amount; set => _Amount = value; }

        private string _Status;
        public string Status { get => _Status; set => _Status = value; }
    }
}