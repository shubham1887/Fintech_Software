using Project.Processing;

namespace Project.Response.eKYC
{
    public class Yes_eKYCInitiateResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
    }
}
