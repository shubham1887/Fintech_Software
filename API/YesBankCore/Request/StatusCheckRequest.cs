using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YESBankCore.Request
{
    public class StatusCheckRequest
    {
        [JsonProperty("OrderNo")]
        public string OrderNo { get; set; }

        internal string IsValidModalState()
        {
            var validator = new StatusCheckRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class StatusCheckRequestValidator : AbstractValidator<StatusCheckRequest>
    {
        public StatusCheckRequestValidator()
        {
            RuleFor(x => x.OrderNo)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "OrderNo"));
        }
    }
}