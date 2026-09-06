using MplanCore.Controller;

namespace MplanCore.Request.MobiKwik
{
    public class PlanViewRequest
    {
        private string _OperatorID;

        public string OperatorID
        {
            get => _OperatorID.TrimIfNotNull();
            set => _OperatorID = value.TrimIfNotNull();
        }
        private string _CircleID;

        public string CircleID
        {
            get => _CircleID.TrimIfNotNull();
            set => _CircleID = value.TrimIfNotNull();
        }

        private string _XMClient;

        public string XMClient
        {
            get => _XMClient.TrimIfNotNull();
            set => _XMClient = value.TrimIfNotNull();
        }
        
    }
}
