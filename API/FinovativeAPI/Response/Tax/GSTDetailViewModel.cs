using Project.Processing;
using Project.Requests.Tax;
using Project.StaticModel;

namespace Project.Response.Tax
{
    public class GSTDetailViewModel
    {
        private string _InvoiceMonth;

        public string InvoiceMonth
        {
            get => _InvoiceMonth.TrimIfNotNull();
            set => _InvoiceMonth = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo.TrimIfNotNull();
            set => _GSTNo = value.TrimIfNotNull();
        }

        private string _InvoiceNo;

        public string InvoiceNo
        {
            get => _InvoiceNo.TrimIfNotNull();
            set => _InvoiceNo = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _GSTType;

        public string GSTType
        {
            get => _GSTType.TrimIfNotNull();
            set => _GSTType = value.TrimIfNotNull();
        }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private decimal _SuccessAmt;

        public decimal SuccessAmt
        {
            get => _SuccessAmt;
            set => _SuccessAmt = value;
        }

        private decimal _DiscountAmount;

        public decimal DiscountAmount
        {
            get => _DiscountAmount;
            set => _DiscountAmount = value;
        }

        private decimal _TaxableValue;

        public decimal TaxableValue
        {
            get => _TaxableValue;
            set => _TaxableValue = value;
        }

        private decimal _IGST;

        public decimal IGST
        {
            get => _IGST;
            set => _IGST = value;
        }

        private decimal _SGST;

        public decimal SGST
        {
            get => _SGST;
            set => _SGST = value;
        }

        private decimal _CGST;

        public decimal CGST
        {
            get => _CGST;
            set => _CGST = value;
        }

        private decimal _GSTTaxableValue;

        public decimal GSTTaxableValue
        {
            get => _GSTTaxableValue;
            set => _GSTTaxableValue = value;
        }

        private string _ServiceCategory;

        public string ServiceCategory
        {
            get => _ServiceCategory.TrimIfNotNull();
            set => _ServiceCategory = value.TrimIfNotNull();
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _SequenceNumber;

        public int SequenceNumber
        {
            get => _SequenceNumber;
            set => _SequenceNumber = value;
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private decimal _GST;

        public decimal GST
        {
            get => _GST;
            set => _GST = value;
        }
        private decimal _CashBack;
        public decimal CashBack
        {
            get => _CashBack;
            set => _CashBack = value;
        }
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private decimal _CCF;
        public decimal CCF
        {
            get => _CCF;
            set => _CCF = value;
        }

        public SelectByCategoryRequest ServiceWise = new SelectByCategoryRequest
        {
            ServiceCategory = InvoiceTypes.ServiceWiseSale,
            Month = 0,
            Year = 0,
            UserID = 0,
            InvoiceNo = "0",
        };
    }
}