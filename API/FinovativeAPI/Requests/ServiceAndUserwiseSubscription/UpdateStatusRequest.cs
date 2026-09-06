using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ServiceAndUserwiseSubscription
{
    public class UpdateStatusRequest
    {
        private int _ServiceAndUserwiseSubscriptionID;

        public int ServiceAndUserwiseSubscriptionID
        {
            get => _ServiceAndUserwiseSubscriptionID;
            set => _ServiceAndUserwiseSubscriptionID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();          
        }
        internal string IsValidModalState()
        {
            var validator = new UpdateStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal class UpdateStatusRequestValidator : AbstractValidator<UpdateStatusRequest>
        {
            public UpdateStatusRequestValidator()
            {
                RuleFor(x => x.ServiceAndUserwiseSubscriptionID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidID);

                RuleFor(x => x.Status)
               .Must(new Validation().IsValidString)
               .WithMessage(Messages.InvalidStatus);
            }
        }

    }
}
