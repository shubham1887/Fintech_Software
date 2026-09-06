using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.ServiceAndUserwiseSubscription
{
    public class PaymentApprovalRequest
    {
        private int _UserServiceSubscriptionChargeID;

        public int UserServiceSubscriptionChargeID
        {
            get => _UserServiceSubscriptionChargeID;
            set => _UserServiceSubscriptionChargeID = value;
        }

        private bool _IsAdminApproved;

        public bool IsAdminApproved
        {
            get => _IsAdminApproved;
            set => _IsAdminApproved = value;
        }

        internal string IsValidModalState()
        {
            var validator = new PaymentApprovalRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class PaymentApprovalRequestValidator : AbstractValidator<PaymentApprovalRequest>
    {
        public PaymentApprovalRequestValidator()
        {
            RuleFor(x => x.UserServiceSubscriptionChargeID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidID);


            RuleFor(x => x.IsAdminApproved)
         .Must(isAdminApproved => isAdminApproved)
         .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.AlreadyApproved);

        }
    }
}