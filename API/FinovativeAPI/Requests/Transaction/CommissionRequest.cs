using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class CommissionRequest
    {
        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
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
            RuleFor(x => x.TransactionMasterID).NotEmpty().WithMessage(TransactionMessage.InvalidTransaction);

            RuleFor(x => x.UserID).NotEmpty().WithMessage(TransactionMessage.InvalidUser);
        }
    }
}