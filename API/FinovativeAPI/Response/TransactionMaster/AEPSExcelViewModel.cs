using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class AEPSExcelViewModel
    {
        private string _TransactionDatetime;

        public string TransactionDatetime
        {
            get => _TransactionDatetime.TrimIfNotNull();
            set => _TransactionDatetime = value.TrimIfNotNull();
        }

        private string _EntityID;

        public string EntityID
        {
            get => _EntityID.TrimIfNotNull();
            set => _EntityID = value.TrimIfNotNull();
        }

        private string _EntityName;

        public string EntityName
        {
            get => _EntityName.TrimIfNotNull();
            set => _EntityName = value.TrimIfNotNull();
        }

        private string _Channel;

        public string Channel
        {
            get => _Channel.TrimIfNotNull();
            set => _Channel = value.TrimIfNotNull();
        }

        private string _ProductName;

        public string ProductName
        {
            get => _ProductName.TrimIfNotNull();
            set => _ProductName = value.TrimIfNotNull();
        }
        public string PartnerName { get; internal set; }


        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }
        
        private string _RRN;

        public string RRN
        {
            get => _RRN.TrimIfNotNull();
            set => _RRN = value.TrimIfNotNull();
        }

        private decimal _TransactionAmount;

        public decimal TransactionAmount
        {
            get => _TransactionAmount;
            set => _TransactionAmount = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private decimal _TDS;
        public decimal TDS
        {
            get => _TDS;
            set => _TDS = value;
        }

        private decimal _NetCommission;

        public decimal NetCommission
        {
            get => _NetCommission;
            set => _NetCommission = value;
        }
        
        private decimal _Charge;

        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }

        private decimal _GST;
        public decimal GST
        {
            get => _GST;
            set => _GST = value;
        }
        
        private decimal _SettlementAmount;
        public decimal SettlementAmount
        {
            get => _SettlementAmount;
            set => _SettlementAmount = value;
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

        private decimal _DScommission;

        public decimal DScommission
        {
            get => _DScommission;
            set => _DScommission = value;
        }

        private decimal _MDScommission;

        public decimal MDScommission
        {
            get => _MDScommission;
            set => _MDScommission = value;
        }


        private string _TxnStatus;

        public string TxnStatus
        {
            get => _TxnStatus.TrimIfNotNull();
            set => _TxnStatus = value.TrimIfNotNull();
        }
        
        private string _CustomerMobileNumber;

        public string CustomerMobileNumber
        {
            get => _CustomerMobileNumber.TrimIfNotNull();
            set => _CustomerMobileNumber = value.TrimIfNotNull();
        }


        private string _AadhaarNumber;

        public string AadhaarNumber
        {
            get => _AadhaarNumber.TrimIfNotNull();
            set => _AadhaarNumber = value.TrimIfNotNull();
        }
        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private int _DSID;

        internal int DSID
        {
            get => _DSID;
            set => _DSID = value;
        }

        private int _MDSID;

        internal int MDSID
        {
            get => _MDSID;
            set => _MDSID = value;
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

        private string _BiometricDevice;
        public string BiometricDevice
        {
            get => _BiometricDevice.TrimIfNotNull();
            set => _BiometricDevice = value.TrimIfNotNull();
        }
        
        private string _STAN;
        public string STAN
        {
            get => _STAN.TrimIfNotNull();
            set => _STAN = value.TrimIfNotNull();
        }

        private string _Remark;
        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }



    }
}