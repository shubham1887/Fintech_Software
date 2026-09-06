using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UpiBeneficiary
{
    public class UpdateRequest
    {
        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private string _UpiBeneficiaryID;

        public string UpiBeneficiaryID
        {
            get => _UpiBeneficiaryID.TrimIfNotNull();
            set => _UpiBeneficiaryID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.UpiBeneficiaryID)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidSender);

            RuleFor(x => x.OTP)
             .Must(new Validation().IsValidString)
             .WithMessage(SenderMessage.InvalidOTP);
        }
    }
}