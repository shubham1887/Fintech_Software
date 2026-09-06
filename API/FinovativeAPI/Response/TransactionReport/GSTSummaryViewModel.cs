using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class GSTSummaryViewModel
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
    }
}