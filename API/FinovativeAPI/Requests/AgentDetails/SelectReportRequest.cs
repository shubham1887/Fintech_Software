using Project.Processing;

namespace Project.Requests.AgentDetails
{
    public class SelectPincodeRequest
    {
        
        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();

            set => _PinCode = value.TrimIfNotNull();
        }
        
    }
}
