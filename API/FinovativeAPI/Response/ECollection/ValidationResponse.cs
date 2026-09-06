using MplanCore.Controller;

namespace Project.Response.ECollection
{
    public class ValidationResponse
    {
        public ValidateResponse validateResponse = new ValidateResponse();

        public class ValidateResponse
        {
            private string _decision;

            public string decision
            {
                get => _decision.TrimIfNotNull();
                set => _decision = value.TrimIfNotNull();
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
