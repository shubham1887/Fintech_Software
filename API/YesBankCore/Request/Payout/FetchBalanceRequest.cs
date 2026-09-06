using FluentValidation.Results;
using FluentValidation;
using YESBankCore.Controller;

namespace YesBankCore.Request.Payout
{
    public class FetchBalanceRequest
    {
        private string _RequestNumber;

        public string RequestNumber
        {
            get => _RequestNumber.TrimIfNotNull();
            set => _RequestNumber = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new FetchBalanceRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }

    }
    internal class FetchBalanceRequestValidator : AbstractValidator<FetchBalanceRequest>
    {
        public FetchBalanceRequestValidator()
        {
            RuleFor(x => x.RequestNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "RequestNumber"));
        }
    }
}
