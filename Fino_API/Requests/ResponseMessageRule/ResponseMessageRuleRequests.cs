using FluentValidation;

namespace FintechApp.Requests.ResponseMessageRule
{
    public class CreateResponseMessageRuleRequest
    {
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public string MatchType { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string Replacement { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string Note { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class CreateResponseMessageRuleRequestValidator : AbstractValidator<CreateResponseMessageRuleRequest>
    {
        public CreateResponseMessageRuleRequestValidator()
        {
            RuleFor(x => x.Pattern).NotEmpty();
        }
    }

    public class UpdateResponseMessageRuleRequest : CreateResponseMessageRuleRequest
    {
        public int ResponseMessageRuleID { get; set; }
    }

    public class UpdateResponseMessageRuleRequestValidator : AbstractValidator<UpdateResponseMessageRuleRequest>
    {
        public UpdateResponseMessageRuleRequestValidator()
        {
            RuleFor(x => x.ResponseMessageRuleID).GreaterThan(0);
            RuleFor(x => x.Pattern).NotEmpty();
        }
    }

    public class GetResponseMessageRuleRequest
    {
        public int ResponseMessageRuleID { get; set; }
    }

    public class DeleteResponseMessageRuleRequest
    {
        public int ResponseMessageRuleID { get; set; }
    }

    /// <summary>
    /// Dry-run: push a sample provider message through the rules exactly as a live response would
    /// go, and report which rule caught it. Nothing is saved and no message is sent.
    /// </summary>
    public class TestResponseMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public string Channel { get; set; } = string.Empty;
    }

    public class TestResponseMessageRequestValidator : AbstractValidator<TestResponseMessageRequest>
    {
        public TestResponseMessageRequestValidator()
        {
            RuleFor(x => x.Message).NotEmpty();
        }
    }
}
