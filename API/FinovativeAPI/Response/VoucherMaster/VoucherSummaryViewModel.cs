namespace Project.Response.VoucherMaster
{
    public class VoucherSummaryViewModel
    {

        private decimal _PaymentAmount;

        public decimal PaymentAmount
        {
            get => _PaymentAmount;
            set => _PaymentAmount = value;
        }
        private decimal _ContraAmount;

        public decimal ContraAmount
        {
            get => _ContraAmount;
            set => _ContraAmount = value;
        }
        private decimal _ReceiptAmount;

        public decimal ReceiptAmount
        {
            get => _ReceiptAmount;
            set => _ReceiptAmount = value;
        }

    }
}
