namespace Project.Response.PayoutBeneficiary
{
    public class CalculateGstResponse
    {
        private decimal _Charges;

        public decimal Charges
        {
            get => _Charges;
            set => _Charges = value;
        }
        private decimal _Gst;

        public decimal Gst
        {
            get => _Gst;
            set => _Gst = value;
        }
      
        private decimal _TDS;
        public decimal TDS
        {
            get => _TDS;
            set => _TDS = value;
        }
        private decimal _Commission;
        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }
        private decimal _TotalAmount;

        public decimal TotalAmount
        {
            get => _TotalAmount;
            set => _TotalAmount = value;
        }

    }
}
