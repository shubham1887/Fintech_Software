namespace Project.Requests.AEPS
{
    public class MosambeeMF919_InitiateRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _Amount;

        public int Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
    }
}