using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class UpdateManualRequest
    {
        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull().ToUpper();
            set => _Status = value.TrimIfNotNull().ToUpper();
        }

        private string _ReferenceNo;

        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }

        private string _Pin;

        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        public override string ToString()
        {
            return $"Status update manually with :Reference No:{ReferenceNo},Status:{Status}";
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateManualRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateManualRequestValidator : AbstractValidator<UpdateManualRequest>
    {
        public UpdateManualRequestValidator()
        {
            RuleFor(x => x.TransactionMasterID)
                    .Must(new Validation().IsValidInt)
                    .WithMessage(Message.TransactionMessage.InvalidTransaction);

            RuleFor(x => x.Status)
                    .Must(new Validation().IsValidString)
                    .WithMessage(Message.TransactionMessage.InvalidStatus);
        }
    }
}