using Project.Processing;

namespace Project.Requests.IPMaster
{
    public class SelectRequest
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
    }
}