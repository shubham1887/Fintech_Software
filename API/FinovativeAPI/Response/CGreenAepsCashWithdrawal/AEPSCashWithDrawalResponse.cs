using Project.Processing;

namespace Project.Response.CGreenAepsCashWithdrawal
{
    public class AEPSCashWithDrawalResponse
    {
        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _message;

        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }        

        private bool _success;

        public bool success
        {
            get => _success;
            set => _success = value;
        }
    }
}
