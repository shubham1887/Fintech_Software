namespace Project.Response.TransactionProviderHistory
{
    public class TransactionHistoryResponse
    {
        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID;
            set => _ProviderRequestID = value;
        }
    }
}