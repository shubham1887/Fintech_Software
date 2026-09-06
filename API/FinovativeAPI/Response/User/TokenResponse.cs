using Project.Processing;

namespace Project.Response.User
{
    public class TokenResponse
    {
        private string _APIToken;

        public string APIToken
        {
            get => _APIToken.TrimIfNotNull();
            set => _APIToken = value.TrimIfNotNull();
        }
    }
}