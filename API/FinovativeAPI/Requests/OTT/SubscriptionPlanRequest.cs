using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OTT
{
    public class SubscriptionPlanRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SubscriptionPlanRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SubscriptionPlanRequestValidator : AbstractValidator<SubscriptionPlanRequest>
    {
        public SubscriptionPlanRequestValidator()
        {
            RuleFor(x => x.OperatorID)
               .Must(new Validation().IsValidInt)
               .WithMessage(OTTMessage.InValidOperator);
        }
    }
}