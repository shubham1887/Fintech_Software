using Project.Processing;

namespace Project.Response.ProviderResponse
{
    public class ProviderReturnResponse
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _balance;

        public string Balance
        {
            get => _balance.TrimIfNotNull();
            set => _balance = value.TrimIfNotNull();
        }

        private string _dmtbalance;

        public string DMTBalance
        {
            get => _dmtbalance.TrimIfNotNull();
            set => _dmtbalance = value.TrimIfNotNull();
        }

        private string _requestID;

        public string RequestID
        {
            get => _requestID.TrimIfNotNull();
            set => _requestID = value.TrimIfNotNull();
        }

        private string _status;

        public string Status
        {
            get => _status.TrimIfNotNull().ToUpper();
            set => _status = value.TrimIfNotNull().ToUpper();
        }

        private string _response;

        public string Response
        {
            get => _response.TrimIfNotNull();
            set => _response = value.TrimIfNotNull();
        }

        private string _Description;

        internal string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _transactionID;

        public string TransactionID
        {
            get => _transactionID.TrimIfNotNull().ToUpperCase();
            set => _transactionID = value.TrimIfNotNull().ToUpperCase();
        }

        public string AmountInString { get; set; }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _LapuNo;

        public string LapuNo
        {
            get => _LapuNo.TrimIfNotNull();
            set => _LapuNo = value.TrimIfNotNull();
        }

        private string _ResponseParam1;

        public string ResponseParam1
        {
            get => _ResponseParam1.TrimIfNotNull();

            set => _ResponseParam1 = value.TrimIfNotNull();
        }

        private string _ResponseParam2;

        public string ResponseParam2
        {
            get => _ResponseParam2.TrimIfNotNull();
            set => _ResponseParam2 = value.TrimIfNotNull();
        }

        private string _ResponseParam3;

        public string ResponseParam3
        {
            get => _ResponseParam3.TrimIfNotNull();
            set => _ResponseParam3 = value.TrimIfNotNull();
        }

        private string _ResponseParam4;

        public string ResponseParam4
        {
            get => _ResponseParam4.TrimIfNotNull();
            set => _ResponseParam4 = value.TrimIfNotNull();
        }

        private string _ResponseParam5;

        public string ResponseParam5
        {
            get => _ResponseParam5.TrimIfNotNull();
            set => _ResponseParam5 = value.TrimIfNotNull();
        }

        private string _ProviderRefNo;

        internal string ProviderRefNo
        {
            get => _ProviderRefNo.TrimIfNotNull();
            set => _ProviderRefNo = value.TrimIfNotNull();
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
        private string _ResponseCode;

        public string ResponseCode
        {
            get => _ResponseCode.TrimIfNotNull();
            set => _ResponseCode = value.TrimIfNotNull();
        }

        private string _CustomerName;

        public string CustomerName
        {
            get => _CustomerName.ToTitleCase();
            set => _CustomerName = value.ToTitleCase();
        }

        private string _DueDate;

        public string DueDate
        {
            get => _DueDate.TrimIfNotNull();
            set => _DueDate = value.TrimIfNotNull();
        }
        
        private string _BillDate;

        public string BillDate
        {
            get => _BillDate.TrimIfNotNull();
            set => _BillDate = value.TrimIfNotNull();
        }

        private string _ProviderCommission;

        public string ProviderCommission
        {
            get => _ProviderCommission.TrimIfNotNull();
            set => _ProviderCommission = value.TrimIfNotNull();
        }

        private bool _IsHitToNewProvider;

        internal bool IsHitToNewProvider
        {
            get => _IsHitToNewProvider;
            set => _IsHitToNewProvider = value;
        }

        private string _BBPSViewID;

        public string BBPSViewID
        {
            get => _BBPSViewID.TrimIfNotNull();
            set => _BBPSViewID = value.TrimIfNotNull();
        }

        private string _ROfferCommission;

        public string ROfferCommission
        {
            get => _ROfferCommission;
            set => _ROfferCommission = value;
        }

        private string _ProviderCommissionPercentage;

        public string ProviderCommissionPercentage
        {
            get => _ProviderCommissionPercentage.TrimIfNotNull();
            set => _ProviderCommissionPercentage = value.TrimIfNotNull();
        }
    }
}