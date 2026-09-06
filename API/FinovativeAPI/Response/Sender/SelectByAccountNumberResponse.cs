using Project.Processing;

namespace Project.Response.Sender
{
    public class SelectByAccountNumberResponse
    {
        private string _SenderNumber;

        public string SenderNumber
        {
            get => _SenderNumber.TrimIfNotNull();
            set => _SenderNumber = value.TrimIfNotNull();
        }

        private string _SenderName;

        public string SenderName
        {
            get => _SenderName.TrimIfNotNull();
            set => _SenderName = value.TrimIfNotNull();
        }
    }
}