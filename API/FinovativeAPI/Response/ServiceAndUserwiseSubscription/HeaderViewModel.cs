namespace Project.Response.ServiceAndUserwiseSubscription
{
    public class HeaderViewModel
    {
        private int _TotalCount;

        public int TotalCount
        {
            get => _TotalCount;
            set => _TotalCount = value;
        }

        private decimal _TotalCharge;

        public decimal TotalCharge
        {
            get => _TotalCharge;
            set => _TotalCharge = value;
        }
    }
}
