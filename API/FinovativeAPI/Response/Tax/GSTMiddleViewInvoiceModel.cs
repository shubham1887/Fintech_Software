namespace Project.Response.Tax
{
    public class GSTMiddleViewInvoiceModel
    {
        private decimal _Qty;

        public decimal Qty
        {
            get => _Qty;
            set => _Qty = value;
        }

        private decimal _DiscountAmount;

        public decimal DiscountAmount
        {
            get => _DiscountAmount;
            set => _DiscountAmount = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName;
            set => _ServiceName = value;
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName;
            set => _FirmName = value;
        }

        private string _HSNCode;

        public string HSNCode
        {
            get => _HSNCode;
            set => _HSNCode = value;
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo;
            set => _GSTNo = value;
        }

        private string _State;

        public string State
        {
            get => _State;
            set => _State = value;
        }

        private decimal _CashBack;
        public decimal CashBack
        {
            get => _CashBack;
            set => _CashBack = value;
        }
    }
}