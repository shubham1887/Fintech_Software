using MplanCore.Controller;

namespace Project.Response.ECollection
{
    public class NotifyResponse
    {
        public NotifyResult notifyResult = new NotifyResult();
        public class NotifyResult
        {
            private string _result;
            public string result
            {
                get => _result.TrimIfNotNull();
                set => _result = value.TrimIfNotNull();
            }
            private string _reject_reason;
            public string reject_reason
            {
                get => _reject_reason.TrimIfNotNull();
                set => _reject_reason = value.TrimIfNotNull();
            }
            
        }
    }
}
