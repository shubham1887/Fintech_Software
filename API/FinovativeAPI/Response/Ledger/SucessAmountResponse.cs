namespace Project.Response.Ledger
{
    public class SucessAmountResponse
    {
        private decimal _Request;

        public decimal Request
        {
            get => _Request;
            set => _Request = value;
        }

        private decimal _Debited;
        public decimal Debited { get => _Debited; set => _Debited = value; }
    }
}