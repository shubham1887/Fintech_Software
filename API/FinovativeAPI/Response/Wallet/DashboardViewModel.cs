namespace Project.Response.Wallet
{
    public class DashboardViewModel
    {
        private decimal _Balance;

        public decimal Balance
        {
            get => _Balance;
            set => _Balance = value;
        }
    }
}