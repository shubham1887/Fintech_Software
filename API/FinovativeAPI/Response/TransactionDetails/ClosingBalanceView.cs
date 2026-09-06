namespace Project.Response.TransactionDetails
{
    public class ClosingBalanceView
    {
        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }
    }
}