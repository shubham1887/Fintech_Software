using Project.Processing;
using System;

namespace Project.Response.PaymentGateway
{
    public class PaymentGatewayViewModel
    {
        private string _PaymentStatus;

        public string PaymentStatus
        {
            get => _PaymentStatus.TrimIfNotNull();
            set => _PaymentStatus = value.TrimIfNotNull();
        }
        private string _DisplayDOC;

        public string DisplayDOC
        {
            get => _DisplayDOC;
            set => _DisplayDOC = value;
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private string _State;
        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _OrderID;

        public string OrderID
        {
            get => _OrderID.TrimIfNotNull();
            set => _OrderID = value.TrimIfNotNull();
        }

        private string _WalletName;

        public string WalletName
        {
            get => _WalletName.TrimIfNotNull();
            set => _WalletName = value.TrimIfNotNull();
        }

        private decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull().ToUpper();
            set => _Status = value.TrimIfNotNull().ToUpper();
        }

        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }

        private string _PayMode;

        public string PayMode
        {
            get => _PayMode.TrimIfNotNull();
            set => _PayMode = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _Sender;

        public string Sender
        {
            get => _Sender.TrimIfNotNull();
            set => _Sender = value.TrimIfNotNull();
        }

        private string _ParentName;

        public string ParentName
        {
            get => _ParentName.TrimIfNotNull();
            set => _ParentName = value.TrimIfNotNull();
        }

        private int _PaymentGatewayID;

        public int PaymentGatewayID
        {
            get => _PaymentGatewayID;
            set => _PaymentGatewayID = value;
        }

        private string _CompanyName;

        public string CompanyName
        {
            get => _CompanyName.TrimIfNotNull();
            set => _CompanyName = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private DateTime _DOC;

        public DateTime DOC
        {
            get => _DOC;
            set => _DOC = value;
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        private int _IsUpdate;

        internal int IsUpdate
        {
            get => _IsUpdate;
            set => _IsUpdate = value;
        }

        private decimal _GatewayCharge;

        public decimal GatewayCharge
        {
            get => _GatewayCharge;
            set => _GatewayCharge = value;
        }

        public int TotalCount { get; internal set; }
        public decimal TotalAmount { get; internal set; }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private decimal _DistributerCommission;

        public decimal DistributerCommission
        {
            get => _DistributerCommission;
            set => _DistributerCommission = value;
        }

        private decimal _SuperDistributerCommission;

        public decimal SuperDistributerCommission
        {
            get => _SuperDistributerCommission;
            set => _SuperDistributerCommission = value;
        }

        private int _DistributerUserID;

        public int DistributerUserID
        {
            get => _DistributerUserID;
            set => _DistributerUserID = value;
        }

        private int _SuperDistributerUserID;

        public int SuperDistributerUserID
        {
            get => _SuperDistributerUserID;
            set => _SuperDistributerUserID = value;
        }

        private string _SuperDistributorName;

        public string SuperDistributorName
        {
            get => _SuperDistributorName.TrimIfNotNull();
            set => _SuperDistributorName = value.TrimIfNotNull();
        }

        private string _DistributorName;

        public string DistributorName
        {
            get => _DistributorName.TrimIfNotNull();
            set => _DistributorName = value.TrimIfNotNull();
        }

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
    }
}