namespace Project.Response.Tax
{
    public class GSTMiddleView
    {
        private decimal _Qty;

        public decimal Qty
        {
            get => _Qty;
            set => _Qty = value;
        }
        private decimal _CCFQty;

        public decimal CCFQty
        {
            get => _CCFQty;
            set => _CCFQty = value;
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

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private string _ServiceCategory;

        public string ServiceCategory
        {
            get => _ServiceCategory;
            set => _ServiceCategory = value;
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
    }
}