namespace Project.Response.ThreewayReconCompletion
{
    public class ViewModel
    {

        private int _ThreewayReconCompletionID;

        public int ThreewayReconCompletionID
        {
            get => _ThreewayReconCompletionID;
            set => _ThreewayReconCompletionID = value;
        }
        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }


        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }


        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

    }
}
