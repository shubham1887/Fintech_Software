using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ServiceAndUserwiseSubscription
{
    public class UpdateRequest
    {
        private int _ServiceAndUserwiseSubscriptionID;

        public int ServiceAndUserwiseSubscriptionID
        {
            get => _ServiceAndUserwiseSubscriptionID;
            set => _ServiceAndUserwiseSubscriptionID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
         
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private decimal _Charge;

        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }

        private decimal _OriginalCharge;

        public decimal OriginalCharge
        {
            get => _OriginalCharge;
            set => _OriginalCharge = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            

            RuleFor(x => x.ServiceAndUserwiseSubscriptionID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidID);


            RuleFor(x => x.Charge)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidCharge);


            RuleFor(x => x.OriginalCharge)
             .Must(new Validation().IsValidDecimal)
             .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidCharge);


        }
    }
}