using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaymentGateway
{
    public class UpdateStatusRequest
    {
        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _UPIID;

        public string UPIID
        {
            get => _UPIID.TrimIfNotNull();
            set => _UPIID = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

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

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateStatusRequestValidator : AbstractValidator<UpdateStatusRequest>
    {
        public UpdateStatusRequestValidator()
        {
            RuleFor(x => x.Amount)
                .Must(new Validation().IsValidString)
                .WithMessage(PaymentGatewayMessage.InvalidAmount);

            RuleFor(x => x.OrderID)
            .Must(new Validation().IsValidString)
            .WithMessage(PaymentGatewayMessage.InvalidOrderID);
        }
    }
}