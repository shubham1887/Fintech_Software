using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaymentGateway
{
    public class TransactionStatusRequest
    {
        private string _OrderID;

        public string OrderID
        {
            get => _OrderID.TrimIfNotNull();
            set => _OrderID = value.TrimIfNotNull();
        }

        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new TransactionStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class TransactionStatusRequestValidator : AbstractValidator<TransactionStatusRequest>
    {
        public TransactionStatusRequestValidator()
        {
            RuleFor(x => x.OrderID)
                .Must(new Validation().IsValidString)
                .WithMessage(PaymentGatewayMessage.InvalidOrderID);
        }
    }
}