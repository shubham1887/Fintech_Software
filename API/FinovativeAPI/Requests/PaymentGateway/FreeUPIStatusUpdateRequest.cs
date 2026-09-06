using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaymentGateway
{
    public class FreeUPIStatusUpdateRequest
    {
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

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new OpenUPIStatusUpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class OpenUPIStatusUpdateRequestValidator : AbstractValidator<FreeUPIStatusUpdateRequest>
    {
        public OpenUPIStatusUpdateRequestValidator()
        {
            RuleFor(x => x.Status)
             .Must(new Validation().IsValidString)
             .WithMessage(PaymentGatewayMessage.InvalidIDStatus);

            RuleFor(x => x.OrderID)
            .Must(new Validation().IsValidString)
            .WithMessage(PaymentGatewayMessage.InvalidOrderID);

            RuleFor(x => x.Message)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidMessage);
        }
    }
}