namespace Project.Requests.Ledger
{
    public class PastResponse
    {
        private decimal _PastFailAmount;

        public decimal PastFailAmount
        {
            get => _PastFailAmount;
            set => _PastFailAmount = value;
        }

        private decimal _PastSuccessAmount;

        public decimal PastSuccessAmount
        {
            get => _PastSuccessAmount;
            set => _PastSuccessAmount = value;
        }
    }
}