using Project.Processing;

namespace Project.Response.AEPS
{
    public class PIDOptionResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _PID;

        public string PID
        {
            get => _PID.TrimIfNotNull();
            set => _PID = value.TrimIfNotNull();
        }
    }
}