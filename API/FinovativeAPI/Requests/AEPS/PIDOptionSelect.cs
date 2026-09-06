using Project.Processing;

namespace Project.Requests.AEPS
{
    public class PIDOptionSelect
    {
        private string _DeviceType;

        public string DeviceType
        {
            get => _DeviceType.TrimIfNotNull();
            set => _DeviceType = value.TrimIfNotNull();
        }

        private string _ActiveAEPS;

        public string ActiveAEPS
        {
            get => _ActiveAEPS.TrimIfNotNull();
            set => _ActiveAEPS = value.TrimIfNotNull();
        }

        private string _TransactionType;

        public string TransactionType
        {
            get => _TransactionType.TrimIfNotNull();
            set => _TransactionType = value.TrimIfNotNull();
        }
    }
}