
using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Service
{
    public class SubscriptionAllowRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private bool _IsSubscriptionAllow;

        public bool IsSubscriptionAllow
        {
            get => _IsSubscriptionAllow;
            set => _IsSubscriptionAllow = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SubscriptionAllowRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Service updated to ServiceID:{ServiceID},IsSubscriptionAllow:{IsSubscriptionAllow}";
        }
    }

    internal class SubscriptionAllowRequestValidator : AbstractValidator<SubscriptionAllowRequest>
    {
        public SubscriptionAllowRequestValidator()
        {
            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.Invalid);
        }
    }
}