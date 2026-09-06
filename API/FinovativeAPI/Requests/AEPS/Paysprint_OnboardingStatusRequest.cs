using Project.Processing;

namespace Project.Requests.AEPS
{
    public class Paysprint_OnboardingStatusRequest
    {
        private string _Pipe;

        public string Pipe
        {
            get => _Pipe.TrimIfNotNull();
            set => _Pipe = value.TrimIfNotNull();
        }
        private int _UserID;
        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

    }
}
