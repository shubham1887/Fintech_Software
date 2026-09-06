namespace Project.Requests.User
{
    public class TwoFAActivateRequest
    {
        private bool _IsTwoFA;

        public bool IsTwoFA
        {
            get => _IsTwoFA;
            set => _IsTwoFA = value;
        }
    }
}