using Project.Processing;

namespace Project.Response.CGreenAepsCashWithdrawal
{
    public class AEPSCashWithDrawalAPIResponse
    {
        private int _status;

        public int status
        {
            get => _status;
            set => _status = value;
        }

        private string _message;

        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }
    }

}
