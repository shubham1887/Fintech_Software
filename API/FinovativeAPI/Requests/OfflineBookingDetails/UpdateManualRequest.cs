using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OfflineBookingDetails
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
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private int _OfflineBookingDetailsID;

        public int OfflineBookingDetailsID
        {
            get => _OfflineBookingDetailsID;
            set => _OfflineBookingDetailsID = value;
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
                    .WithMessage(TransactionMessage.InvalidTransaction);

            RuleFor(x => x.Status)
                    .Must(new Validation().IsValidString)
                    .WithMessage(TransactionMessage.InvalidStatus);
        }
    }
}