using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.ServiceAndUserwiseSubscription
{
    public class AutoDebitOnOffRequest
    {

        private int _ServiceAndUserwiseSubscriptionID;

        public int ServiceAndUserwiseSubscriptionID
        {
            get => _ServiceAndUserwiseSubscriptionID;
            set => _ServiceAndUserwiseSubscriptionID = value;
        }

        private bool _IsAutoDebit;

        public bool IsAutoDebit
        {
            get => _IsAutoDebit;
            set => _IsAutoDebit = value;
        }

        internal string IsValidModalState()
        {
            var validator = new AutoDebitOnOffRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AutoDebitOnOffRequestValidator : AbstractValidator<AutoDebitOnOffRequest>
    {
        public AutoDebitOnOffRequestValidator()
        {
            RuleFor(x => x.ServiceAndUserwiseSubscriptionID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidID);

        }
    }
}
