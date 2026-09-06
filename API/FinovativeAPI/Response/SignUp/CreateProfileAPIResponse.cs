using Project.Processing;

namespace Project.Response.SignUp
{
    public class CreateProfileAPIResponse
    {
        private string _capture_link;

        public string capture_link
        {
            get => _capture_link.TrimIfNotNull();
            set => _capture_link = value.TrimIfNotNull();
        }
        private string _profile_id;

        public string profile_id
        {
            get => _profile_id.TrimIfNotNull();
            set => _profile_id = value.TrimIfNotNull();
        }
        private string _error;

        public string error
        {
            get => _error.TrimIfNotNull();
            set => _error = value.TrimIfNotNull();
        }
        private string _message;

        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

    }
}
