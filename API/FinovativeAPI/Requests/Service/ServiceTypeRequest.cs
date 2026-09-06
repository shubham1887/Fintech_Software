using Project.Processing;

namespace Project.Requests.Service
{
    public class ServiceTypeRequest
    {
        private string _ServiceType;

        public string ServiceType
        {
            get => _ServiceType.TrimIfNotNullWithDefault(StaticModel.ServiceType.Recharge);
            set => _ServiceType = value.TrimIfNotNullWithDefault(StaticModel.ServiceType.Recharge);
        }
    }
}