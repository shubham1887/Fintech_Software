using Project.Processing;

namespace project.Response.User
{
    public class ProfileUploadResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _ProfileImagePath;

        public string ProfileImagePath
        {
            get => _ProfileImagePath.TrimIfNotNull();
            set => _ProfileImagePath = value.TrimIfNotNull();
        }
    }
}