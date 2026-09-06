using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class ShiftRequest
    {
        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private bool _IsStatusCheck;

        public bool IsStatusCheck
        {
            get => _IsStatusCheck;
            set => _IsStatusCheck = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ShiftRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Shift recharge with :TransactionMasterID:{TransactionMasterID},ProviderID:{ProviderID}";
        }
    }

    internal class ShiftRequestValidator : AbstractValidator<ShiftRequest>
    {
        public ShiftRequestValidator()
        {
            RuleFor(x => x.TransactionMasterID)
               .Must(new Validation().IsValidInt)
               .WithMessage(TransactionMessage.InvalidID);

            RuleFor(x => x.ProviderID)
              .Must(new Validation().IsValidInt)
              .WithMessage(ProviderMessage.InValidProvider);
        }
    }
}