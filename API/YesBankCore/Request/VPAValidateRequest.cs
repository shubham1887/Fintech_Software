using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YESBankCore.Request
{
    public class VPAValidateRequest
    {
        [JsonProperty("OrderNo")]
        public string OrderNo { get; set; }

        [JsonProperty("VPA")]
        public string VPA { get; set; }

        internal string IsValidModalState()
        {
            var validator = new VPAValidateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class VPAValidateRequestValidator : AbstractValidator<VPAValidateRequest>
    {
        public VPAValidateRequestValidator()
        {
            RuleFor(x => x.OrderNo)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "OrderNo"));

            RuleFor(x => x.VPA)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "VPA"));
        }
    }
}