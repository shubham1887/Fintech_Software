using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class ResendRequest
    {
        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private bool _IsStatusCheck;

        public bool IsStatusCheck
        {
            get => _IsStatusCheck;
            set => _IsStatusCheck = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ResendRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Resend transaction with :TransactionMasterID:{TransactionMasterID}";
        }
    }

    internal class ResendRequestValidator : AbstractValidator<ResendRequest>
    {
        public ResendRequestValidator()
        {
            RuleFor(x => x.TransactionMasterID)
               .Must(new Validation().IsValidInt)
               .WithMessage(TransactionMessage.InvalidID);
        }
    }
}