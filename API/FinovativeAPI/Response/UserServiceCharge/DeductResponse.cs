using Project.Processing;

namespace Project.Response.UserServiceCharge
{
    public class DeductResponse
    {
        private bool _IsChargeDeducted;

        public bool IsChargeDeducted
        {
            get => _IsChargeDeducted;
            set => _IsChargeDeducted = value;
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
    }
}