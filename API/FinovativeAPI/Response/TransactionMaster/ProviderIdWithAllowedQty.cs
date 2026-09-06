using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class ProviderIdWithAllowedQty
    {
        private int _ProviderID;

        private string _AllowedQty;

        public int ProviderID { get => _ProviderID; set => _ProviderID = value; }
        public string AllowedQty { get => _AllowedQty.TrimIfNotNull(); set => _AllowedQty = value.TrimIfNotNull(); }
    }
}