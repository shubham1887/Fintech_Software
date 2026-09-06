using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PanCard
{
    public class PanStatusRequest
    {
        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }
        internal string IsValidModalState()
        {
            var validator = new Paysprint_PanStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }
    internal class Paysprint_PanStatusRequestValidator : AbstractValidator<PanStatusRequest>
    {
        public Paysprint_PanStatusRequestValidator()
        {
            RuleFor(x => x.TransactionMasterID)
               .Must(new Validation().IsValidInt)
               .WithMessage(PanMessage.InvalidTransactionID);

        }
    }
}