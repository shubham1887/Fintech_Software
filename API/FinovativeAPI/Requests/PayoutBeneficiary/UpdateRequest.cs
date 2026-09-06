using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PayoutBeneficiary
{
    public class UpdateRequest
    {
        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private int _BeneficiaryID;

        public int BeneficiaryID
        {
            get => _BeneficiaryID;
            set => _BeneficiaryID = value;
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
            RuleFor(x => x.BeneficiaryID)
               .Must(new Validation().IsValidInt)
               .WithMessage(BeneficiaryMessage.InvalidBeneficiary);

            RuleFor(x => x.OTP)
             .Must(new Validation().IsValidString)
             .WithMessage(SenderMessage.InvalidOTP);
        }
    }
}