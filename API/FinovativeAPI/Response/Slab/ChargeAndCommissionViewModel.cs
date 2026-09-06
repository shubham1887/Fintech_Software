namespace Project.Response.Slab
{
    public class ChargeAndCommissionViewModel
    {
        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private decimal _Charge;

        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }

        private decimal _Total;

        public decimal Total
        {
            get => _Total;
            set => _Total = value;
        }
    }
}