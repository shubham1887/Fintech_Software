using FluentValidation.Results;
using FluentValidation;
using YESBankCore.Controller;

namespace YesBankCore.Request.Payout
{
    public class PayoutStatusCheckRequest
    {
        private string _RequestNumber;

        public string RequestNumber
        {
            get => _RequestNumber.TrimIfNotNull();
            set => _RequestNumber = value.TrimIfNotNull();
        }

        private string _PartnerReferenceNumber;

        public string PartnerReferenceNumber
        {
            get => _PartnerReferenceNumber.TrimIfNotNull();
            set => _PartnerReferenceNumber = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new PayoutStatusCheckRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }

    }
    internal class PayoutStatusCheckRequestValidator : AbstractValidator<PayoutStatusCheckRequest>
    {
        public PayoutStatusCheckRequestValidator()
        {
            RuleFor(x => x.RequestNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "RequestNumber"));

            RuleFor(x => x.PartnerReferenceNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "PartnerReferenceNumber"));


        }
    }
}
