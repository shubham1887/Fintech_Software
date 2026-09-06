using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class OperatorWiseGSTViewModel
    {
        private string _InvoiceMonth;

        public string InvoiceMonth
        {
            get => _InvoiceMonth.TrimIfNotNull();
            set => _InvoiceMonth = value.TrimIfNotNull();
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
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

        private decimal _TaxableValue;

        public decimal TaxableValue
        {
            get => _TaxableValue;
            set => _TaxableValue = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _DCommission;

        public decimal DCommission
        {
            get => _DCommission;
            set => _DCommission = value;
        }

        private decimal _SDCommission;

        public decimal SDCommission
        {
            get => _SDCommission;
            set => _SDCommission = value;
        }

        private decimal _RetailerCommission;

        public decimal RetailerCommission
        {
            get => _RetailerCommission;
            set => _RetailerCommission = value;
        }

        private decimal _Charge;

        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }

        private decimal _GivenCommission;

        public decimal GivenCommission
        {
            get => _GivenCommission;
            set => _GivenCommission = value;
        }

        private string _Percentage;

        public string Percentage
        {
            get => _Percentage.TrimIfNotNull();
            set => _Percentage = value.TrimIfNotNull();
        }

        private string _CommLess;

        public string CommLess
        {
            get => _CommLess.TrimIfNotNull();
            set => _CommLess = value.TrimIfNotNull();
        }

        private string _ReviceGST;

        public string ReviceGST
        {
            get => _ReviceGST.TrimIfNotNull();
            set => _ReviceGST = value.TrimIfNotNull();
        }
    }
}