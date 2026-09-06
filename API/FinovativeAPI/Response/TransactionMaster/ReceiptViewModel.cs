using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class ReceiptViewModel
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

        private string _FirmNameWithNumber;

        public string FirmNameWithNumber
        {
            get => _FirmNameWithNumber.TrimIfNotNull();
            set => _FirmNameWithNumber = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _SenderNo;

        public string SenderNo
        {
            get => _SenderNo.TrimIfNotNull();
            set => _SenderNo = value.TrimIfNotNull();
        }

        private string _BeneName;

        public string BeneName
        {
            get => _BeneName.TrimIfNotNull();
            set => _BeneName = value.TrimIfNotNull();
        }

        private string _Bank;

        public string Bank
        {
            get => _Bank.TrimIfNotNull();
            set => _Bank = value.TrimIfNotNull();
        }

        private decimal _TotalAmount;

        public decimal TotalAmount
        {
            get => _TotalAmount;
            set => _TotalAmount = value;
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _Charge;

        public string Charge
        {
            get => _Charge.TrimIfNotNull();
            set => _Charge = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        internal decimal AmountDecimal { set; get; }

        internal decimal ChargeDecimal { set; get; }

        internal decimal GSTDecimal { set; get; }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }

        private string _PaymentType;

        public string PaymentType
        {
            get => _PaymentType.TrimIfNotNull();
            set => _PaymentType = value.TrimIfNotNull();
        }

        private string _FirmAddress;

        public string FirmAddress
        {
            get => _FirmAddress.TrimIfNotNull();
            set => _FirmAddress = value.TrimIfNotNull();
        }

        private string _CompanyName;

        public string CompanyName
        {
            get => _CompanyName.TrimIfNotNull();
            set => _CompanyName = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }
    }
}