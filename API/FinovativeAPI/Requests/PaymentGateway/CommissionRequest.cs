using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaymentGateway
{
    public class CommissionRequest
    {
        private int _PaymentGatewayID;

        public int PaymentGatewayID
        {
            get => _PaymentGatewayID;
            set => _PaymentGatewayID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new CommissionRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success)
            {
                return StatusType.Ok;
            }

            return failures[0].ToString();
        }
    }

    internal class CommissionRequestValidator : AbstractValidator<CommissionRequest>
    {
        public CommissionRequestValidator()
        {
            RuleFor(x => x.PaymentGatewayID).NotEmpty().WithMessage(PaymentGatewayMessage.InvalidPaymentGatewayID);

            RuleFor(x => x.UserID).NotEmpty().WithMessage(TransactionMessage.InvalidUser);
        }
    }
}