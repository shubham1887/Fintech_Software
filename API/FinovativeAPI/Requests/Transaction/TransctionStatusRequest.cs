using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class TransctionStatusRequest
    {
        private int transactionMasterID;

        public int TransactionMasterID
        {
            get => transactionMasterID;
            set => transactionMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new TransctionStatusRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class TransctionStatusRequestValidator : AbstractValidator<TransctionStatusRequest>
    {
        public TransctionStatusRequestValidator()
        {
            RuleFor(x => x.TransactionMasterID)
               .Must(new Validation().IsValidInt)
               .WithMessage(TransactionMessage.InvalidRequestID);
        }
    }
}