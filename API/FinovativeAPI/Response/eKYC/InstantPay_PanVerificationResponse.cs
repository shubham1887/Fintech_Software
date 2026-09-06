using Project.Processing;

namespace Project.Response.eKYC
{
    public class InstantPay_PanVerificationResponse
    {
        private string _Message;
        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Name;
        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }
    }
}
