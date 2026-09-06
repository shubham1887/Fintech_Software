using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SMSMaster
{
    public class BalanceRequest
    {
        private string _BalanceURL;

        public string BalanceURL
        {
            get => _BalanceURL.TrimIfNotNull();
            set => _BalanceURL = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new BalanceRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class BalanceRequestValidator : AbstractValidator<BalanceRequest>
    {
        public BalanceRequestValidator()
        {
            RuleFor(x => x.BalanceURL)
              .Must(new Validation().IsValidString)
              .WithMessage(SMSMasterMessage.InvalidBalUrl);
        }
    }
}