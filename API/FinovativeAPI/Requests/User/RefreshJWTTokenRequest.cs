using Project.Processing;

namespace Project.Requests.User
{
    public class RefreshJWTTokenRequest
    {
        private string _RefreshToken;

        public string RefreshToken
        {
            get => _RefreshToken.TrimIfNotNull();
            set => _RefreshToken = value.TrimIfNotNull();
        }
    }
}