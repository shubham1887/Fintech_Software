using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaymentGateway
{
    public class SelectOrderIDRequest
    {
        private string _OrderID;

        public string OrderID
        {
            get => _OrderID.TrimIfNotNull();
            set => _OrderID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectTransactionIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectTransactionIDRequestValidator : AbstractValidator<SelectOrderIDRequest>
    {
        public SelectTransactionIDRequestValidator()
        {
            RuleFor(x => x.OrderID)
                .Must(new Validation().IsValidString)
                .WithMessage(PaymentGatewayMessage.InvalidOrderID);
        }
    }
}