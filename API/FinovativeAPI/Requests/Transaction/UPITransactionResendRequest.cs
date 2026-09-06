using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class UPITransactionResendRequest
    {
        private string _ID;

        public string ID
        {
            get => _ID.TrimIfNotNull();
            set => _ID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UPITransactionResendRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UPITransactionResendRequestValidator : AbstractValidator<UPITransactionResendRequest>
    {
        public UPITransactionResendRequestValidator()
        {
            RuleFor(x => x.ID)
               .Must(new Validation().IsValidString)
               .WithMessage(TransactionMessage.InvalidRequestID);
        }
    }
}