using Project.Processing;

namespace Project.Response
{
    public class APIResponse
    {
        private bool _Status;

        public bool Status
        {
            get => _Status; set => _Status = value;
        }

        private string _Body;

        public string Body
        {
            get => _Body.TrimIfNotNull();
            set => _Body = value.TrimIfNotNull();
        }
    }
}