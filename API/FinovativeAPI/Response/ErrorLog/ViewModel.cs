using Project.Processing;

namespace Project.Response.ErrorLog
{
    public class ViewModel
    {
        private string _Exception;

        public string Exception
        {
            get => _Exception.TrimIfNotNull();
            set => _Exception = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
    }
}