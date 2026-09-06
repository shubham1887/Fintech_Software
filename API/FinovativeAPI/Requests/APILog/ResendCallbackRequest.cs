namespace Project.Requests.APILog
{
    public class ResendCallbackRequest
    {
        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }
    }
}