using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.ServiceAndUserwiseSubscription
{
    public class PaymentDueRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
        internal string IsValidModalState()
        {
            var validator = new PaymentDueRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class PaymentDueRequestValidator : AbstractValidator<PaymentDueRequest>
    {
        public PaymentDueRequestValidator()
        {

            RuleFor(x => x.ServiceID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidServiceID);

            


        }
    }
}