namespace project.Requests.Transaction
{
    public class ProviderWisePerformanceGraphRequest
    {

        private string _Date;

        public string Date
        {
            get => _Date;
            set => _Date = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }
    }
}
