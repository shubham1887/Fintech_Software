using Project.Processing;

namespace Project.Requests.PublicAPI
{
    public class GetOperatorByMobileNumberRequest
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }
    }
}