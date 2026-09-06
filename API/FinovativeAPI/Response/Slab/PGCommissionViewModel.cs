namespace Project.Response.Slab
{
    public class PGCommissionViewModel
    {
        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory;
            set => _CommissionCategory = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }
    }
}