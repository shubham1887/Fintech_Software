namespace Project.Response.ProviderPurchase
{
    public class PurchaseViewModel
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private decimal _Purchase;

        public decimal Purchase
        {
            get => _Purchase;
            set => _Purchase = value;
        }
    }
}