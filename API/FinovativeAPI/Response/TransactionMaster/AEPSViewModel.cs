using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class AEPSViewModel
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
        
        private string _AgentID;

        public string AgentID
        {
            get => _AgentID.TrimIfNotNull();
            set => _AgentID = value.TrimIfNotNull();
        }
        
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }
        public string ProviderName { get; internal set; }


        private string _RequestID;

        public string RequestID
        {
            get => _RequestID.TrimIfNotNull();
            set => _RequestID = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _RetailerComm;

        public decimal RetailerComm
        {
            get => _RetailerComm;
            set => _RetailerComm = value;
        }

        private decimal _TDS;
        public decimal TDS
        {
            get => _TDS;
            set => _TDS = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }

        private string _Remark;
        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }
        private string _DeviceType;
        public string DeviceType
        {
            get => _DeviceType.TrimIfNotNull();
            set => _DeviceType = value.TrimIfNotNull();
        }


        private string _Logo;

        public string Logo
        {
            get => _Logo.TrimIfNotNull();
            set => _Logo = value.TrimIfNotNull();
        }

        

        private string _CustomerNumber;

        public string CustomerNumber
        {
            get => _CustomerNumber.TrimIfNotNull();
            set => _CustomerNumber = value.TrimIfNotNull();
        }

       

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        public string Through { get; internal set; }

        private string _IPAddress;

        public string IPAddress
        {
            get => _IPAddress.TrimIfNotNull();
            set => _IPAddress = value.TrimIfNotNull();
        }

        

        private decimal _DebitAmount;

        public decimal DebitAmount
        {
            get => _DebitAmount;
            set => _DebitAmount = value;
        }

        private decimal _DistributerComm;

        public decimal DistributerComm
        {
            get => _DistributerComm;
            set => _DistributerComm = value;
        }

        private decimal _SuperDistributerComm;

        public decimal SuperDistributerComm
        {
            get => _SuperDistributerComm;
            set => _SuperDistributerComm = value;
        }

        private string _UpdatedDOC;

        public string UpdatedDOC
        {
            get => _UpdatedDOC.TrimIfNotNull();
            set => _UpdatedDOC = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        public string ComplaintStatus { get; internal set; }

        private int _DistributerUserID;

        internal int DistributerUserID
        {
            get => _DistributerUserID;
            set => _DistributerUserID = value;
        }

        private int _SuperDistributerUserID;

        internal int SuperDistributerUserID
        {
            get => _SuperDistributerUserID;
            set => _SuperDistributerUserID = value;
        }

        private string _UserNumber;

        public string UserNumber
        {
            get => _UserNumber.TrimIfNotNull();
            set => _UserNumber = value.TrimIfNotNull();
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

        private decimal _ProviderCommission;

        public decimal ProviderCommission
        {
            get => _ProviderCommission;
            set => _ProviderCommission = value;
        }

        private decimal _GST;

        public decimal GST
        {
            get => _GST;
            set => _GST = value;
        }

        private decimal _AdminProfit;

        public decimal AdminProfit
        {
            get => _AdminProfit;
            set => _AdminProfit = value;
        }

        private string _SenderNumber;

        public string SenderNumber
        {
            get => _SenderNumber.TrimIfNotNull();
            set => _SenderNumber = value.TrimIfNotNull();
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
        
    }
}