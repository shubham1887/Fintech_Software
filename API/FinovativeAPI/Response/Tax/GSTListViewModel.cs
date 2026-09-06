namespace Project.Response.Tax
{
    public class GSTListViewModel
    {
        private string _GSTType;

        public string GSTType
        {
            get => _GSTType;
            set => _GSTType = value;
        }

        private decimal _CGST;

        public decimal CGST
        {
            get => _CGST;
            set => _CGST = value;
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

        private decimal _GSTTaxableValue;

        public decimal GSTTaxableValue
        {
            get => _GSTTaxableValue;
            set => _GSTTaxableValue = value;
        }

        private decimal _TaxableValue;

        public decimal TaxableValue
        {
            get => _TaxableValue;
            set => _TaxableValue = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName;
            set => _ServiceName = value;
        }
        private decimal _CashBack;
        public decimal CashBack
        {
            get => _CashBack;
            set => _CashBack = value;
        }

        private decimal _CCF;
        public decimal CCF
        {
            get => _CCF;
            set => _CCF = value;
        }
    }
}