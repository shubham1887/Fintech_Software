using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.UpiBeneficiary
{
    public class DeleteRequest
    {
        private string _ActiveUPITransferName;

        [DefaultValue("")]
        public string ActiveUPITransferName
        {
            get => _ActiveUPITransferName.TrimIfNotNull();
            set => _ActiveUPITransferName = value.TrimIfNotNull();
        }

        private string _UpiBeneficiaryID;

        public string UpiBeneficiaryID
        {
            get => _UpiBeneficiaryID.TrimIfNotNull();
            set => _UpiBeneficiaryID = value.TrimIfNotNull();
        }

        private string _UpiSenderID;

        public string UpiSenderID
        {
            get => _UpiSenderID.TrimIfNotNull();
            set => _UpiSenderID = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.UpiBeneficiaryID)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidBeneficiary);

            //RuleFor(x => x.MobileNumber)
            // .Must(new Validation().IsValidString)
            // .WithMessage(SenderMessage.InvalidSender);
        }
    }
}