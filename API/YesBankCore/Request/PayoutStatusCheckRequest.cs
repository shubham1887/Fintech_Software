using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request
{
    public class PayoutStatusCheckRequest
    {
        private string _OrderID;

        public string orderID
        {
            get => _OrderID.TrimIfNotNull();
            set => _OrderID = value.TrimIfNotNull();
        }

        private string _accountNumber;

        public string accountNumber
        {
            get => _accountNumber.TrimIfNotNull();
            set => _accountNumber = value.TrimIfNotNull();
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
            RuleFor(x => x.orderID)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "orderID"));
        }
    }
}