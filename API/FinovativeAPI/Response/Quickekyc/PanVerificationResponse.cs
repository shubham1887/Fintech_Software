using Project.Processing;

namespace Project.Response.Quickekyc
{
    public class PanVerificationResponse
    {
        private string _full_name;

        public string full_name
        {
            get => _full_name.TrimIfNotNull();
            set => _full_name = value.TrimIfNotNull();
        }

        private string _category;

        public string category
        {
            get => _category.TrimIfNotNull();
            set => _category = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
    }
}