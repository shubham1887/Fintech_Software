using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class ViewModel
    {
        public int TransactionMasterID { set; get; }

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
        
        private string _AgentID;

        public string AgentID
        {
            get => _AgentID.TrimIfNotNull();
            set => _AgentID = value.TrimIfNotNull();
        }
        
        private string _AgentName;

        public string AgentName
        {
            get => _AgentName.TrimIfNotNull();
            set => _AgentName = value.TrimIfNotNull();
        }

        private string _UserMobile;

        public string UserMobile
        {
            get => _UserMobile.TrimIfNotNull();
            set => _UserMobile = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        public decimal Amount { set; get; }

        private decimal _ROfferCommission;

        public decimal ROfferCommission
        {
            get => _ROfferCommission;
            set => _ROfferCommission = value;
        }

        private decimal _RetailerCommission;

        public decimal RetailerCommission
        {
            get => _RetailerCommission;
            set => _RetailerCommission = value;
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

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _BeneName;

        public string BeneName
        {
            get => _BeneName.TrimIfNotNull();
            set => _BeneName = value.TrimIfNotNull();
        }

        public decimal OpeningBalance { get; set; }

        public decimal DebitAmount { set; get; }
        public decimal ClosingBalance { get; set; }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.ToUpperCase();
            set => _ReferenceNo = value.ToUpperCase();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.TrimIfNotNull();
            set => _ProviderRequestID = value.TrimIfNotNull();
        }

        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _PaymentType;

        public string PaymentType
        {
            get => _PaymentType.TrimIfNotNull();
            set => _PaymentType = value.TrimIfNotNull();
        }

        private string _SenderNo;

        public string SenderNo
        {
            get => _SenderNo.TrimIfNotNull();
            set => _SenderNo = value.TrimIfNotNull();
        }

        private string _SenderName;

        public string SenderName
        {
            get => _SenderName.TrimIfNotNull();
            set => _SenderName = value.TrimIfNotNull();
        }

        private string _BeneficaryNameByBank;

        public string BeneficaryNameByBank
        {
            get => _BeneficaryNameByBank.TrimIfNotNull();
            set => _BeneficaryNameByBank = value.TrimIfNotNull();
        }

        private string _CommissionInfo;

        public string CommissionInfo
        {
            get => _CommissionInfo;
            set => _CommissionInfo = value;
        }

        private decimal _TDS;

        public decimal TDS
        {
            get => _TDS;
            set => _TDS = value;
        }

        private decimal _CashBack;

        public decimal CashBack
        {
            get => _CashBack;
            set => _CashBack = value;
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _IPAddress;

        public string IPAddress
        {
            get => _IPAddress.TrimIfNotNull();
            set => _IPAddress = value.TrimIfNotNull();
        }

        private double _ProcTime;

        public double ProcTime
        {
            get => _ProcTime;
            set => _ProcTime = value;
        }

        public int UserID { set; get; }

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

        private string _Logo;

        public string Logo
        {
            get => _Logo.TrimIfNotNull();
            set => _Logo = value.TrimIfNotNull();
        }

        private string _Bank;

        public string Bank
        {
            get => _Bank.TrimIfNotNull();
            set => _Bank = value.TrimIfNotNull();
        }

        private string _ComplaintStatus;

        public string ComplaintStatus
        {
            get => _ComplaintStatus.TrimIfNotNull();
            set => _ComplaintStatus = value.TrimIfNotNull();
        }

        public int OperatorID { set; get; }
        public int ServiceID { set; get; }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        public int ProviderID { get; internal set; }

        private string _Circle;

        public string Circle
        {
            get => _Circle.TrimIfNotNull();
            set => _Circle = value.TrimIfNotNull();
        }

        private string _Through;

        public string Through
        {
            get => _Through.TrimIfNotNull();
            set => _Through = value.TrimIfNotNull();
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private decimal _ProviderClosingBalance;

        public decimal ProviderClosingBalance
        {
            get => _ProviderClosingBalance;
            set => _ProviderClosingBalance = value;
        }

        private int _SuperDistributerUserID;

        internal int SuperDistributerUserID
        {
            get => _SuperDistributerUserID;
            set => _SuperDistributerUserID = value;
        }

        private decimal _GST;

        public decimal GST
        {
            get => _GST;
            set => _GST = value;
        }

        private decimal _DistributerGST;

        public decimal DistributerGST
        {
            get => _DistributerGST;
            set => _DistributerGST = value;
        }

        private decimal _DistributerTDS;

        public decimal DistributerTDS
        {
            get => _DistributerTDS;
            set => _DistributerTDS = value;
        }

        private decimal _SuperDistributerGST;

        public decimal SuperDistributerGST
        {
            get => _SuperDistributerGST;
            set => _SuperDistributerGST = value;
        }

        private decimal _SuperDistributerTDS;

        public decimal SuperDistributerTDS
        {
            get => _SuperDistributerTDS;
            set => _SuperDistributerTDS = value;
        }

        private int _DistributerUserID;

        internal int DistributerUserID
        {
            get => _DistributerUserID;
            set => _DistributerUserID = value;
        }

        private decimal _ProviderCommission;

        public decimal ProviderCommission
        {
            get => _ProviderCommission;
            set => _ProviderCommission = value;
        }

        private string _UpdatedDOC;

        public string UpdatedDOC
        {
            get => _UpdatedDOC.TrimIfNotNull();
            set => _UpdatedDOC = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        private string _TxnID;

        public string TxnID
        {
            get => _TxnID.TrimIfNotNull();
            set => _TxnID = value.TrimIfNotNull();
        }

        private string _AmountInfo;

        public string AmountInfo
        {
            get => _AmountInfo.TrimIfNotNull();
            set => _AmountInfo = value.TrimIfNotNull();
        }
        
        private string _RetailerCommPercentage;

        public string RetailerCommPercentage
        {
            get => _RetailerCommPercentage;
            set => _RetailerCommPercentage = value;
        }

        private string _BalanceInfo;

        public string BalanceInfo
        {
            get => _BalanceInfo.TrimIfNotNull();
            set => _BalanceInfo = value.TrimIfNotNull();
        }

        private string _ProviderInfo;

        public string ProviderInfo
        {
            get => _ProviderInfo.TrimIfNotNull();
            set => _ProviderInfo = value.TrimIfNotNull();
        }

        private string _APIBalInfo;

        public string APIBalInfo
        {
            get => _APIBalInfo.TrimIfNotNull();
            set => _APIBalInfo = value.TrimIfNotNull();
        }

        private decimal _AdminCommission;

        public decimal AdminCommission
        {
            get => _AdminCommission;
            set => _AdminCommission = value;
        }

        private decimal _AdminProfit;

        public decimal AdminProfit
        {
            get => _AdminProfit;
            set => _AdminProfit = value;
        }

        private string _ResponseType;

        public string ResponseType
        {
            get => _ResponseType;
            set => _ResponseType = value;
        }

        private string _ProviderRefNo;

        public string ProviderRefNo
        {
            get => _ProviderRefNo;
            set => _ProviderRefNo = value;
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }
        
        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }
        
        private decimal _Charge;

        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }
        
        private decimal _NetCommission;

        public decimal NetCommission
        {
            get => _NetCommission;
            set => _NetCommission = value;
        }

        private string _Latitude;
        public string Latitude
        {
            get => _Latitude.TrimIfNotNull();
            set => _Latitude = value.TrimIfNotNull();
        }

        private string _Longitude;

        public string Longitude
        {
            get => _Longitude.TrimIfNotNull();
            set => _Longitude = value.TrimIfNotNull();
        }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }
    }
}
