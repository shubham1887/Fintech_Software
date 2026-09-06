using Project.Processing;

namespace Project.Response.GSTHold
{
    public class GSTViewModel
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

        private string _InvoiceType;

        public string InvoiceType
        {
            get => _InvoiceType.TrimIfNotNull();
            set => _InvoiceType = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _LastYearMonth;

        public string LastYearMonth
        {
            get => _LastYearMonth.TrimIfNotNull();
            set => _LastYearMonth = value.TrimIfNotNull();
        }

        private decimal _ReverseCharge;

        public decimal ReverseCharge
        {
            get => _ReverseCharge;
            set => _ReverseCharge = value;
        }

        private decimal _Rate;

        public decimal Rate
        {
            get => _Rate;
            set => _Rate = value;
        }

        private decimal _InvoiceValue;

        public decimal InvoiceValue
        {
            get => _InvoiceValue;
            set => _InvoiceValue = value;
        }

        private decimal _TaxableValue;

        public decimal TaxableValue
        {
            get => _TaxableValue;
            set => _TaxableValue = value;
        }

        private decimal _HoldAmount;

        public decimal HoldAmount
        {
            get => _HoldAmount;
            set => _HoldAmount = value;
        }

        private decimal _IGST;

        public decimal IGST
        {
            get => _IGST;
            set => _IGST = value;
        }

        private decimal _CGST;

        public decimal CGST
        {
            get => _CGST;
            set => _CGST = value;
        }

        private decimal _SGST;

        public decimal SGST
        {
            get => _SGST;
            set => _SGST = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private int _Year;

        public int Year
        {
            get => _Year;
            set => _Year = value;
        }

        private int _Month;

        public int Month
        {
            get => _Month;
            set => _Month = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
    }
}