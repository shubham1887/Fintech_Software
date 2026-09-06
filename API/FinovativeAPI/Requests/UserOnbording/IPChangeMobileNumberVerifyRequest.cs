using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserOnbording
{
    public class IPChangeMobileNumberVerifyRequest
    {
        private string _OldMobileNumber;

        public string OldMobileNumber
        {
            get { return _OldMobileNumber.TrimIfNotNull(); }
            set { _OldMobileNumber = value.TrimIfNotNull(); }
        }

        private string _NewMobileNumber;

        public string NewMobileNumber
        {
            get { return _NewMobileNumber.TrimIfNotNull(); }
            set { _NewMobileNumber = value.TrimIfNotNull(); }
        }

        private string _OldOTP;

        public string OldOTP
        {
            get { return _OldOTP.TrimIfNotNull(); }
            set { _OldOTP = value.TrimIfNotNull(); }
        }

        private string _NewOTP;

        public string NewOTP
        {
            get { return _NewOTP.TrimIfNotNull(); }
            set { _NewOTP = value.TrimIfNotNull(); }
        }

        internal string IsValidModalState()
        {
            var validator = new IPChangeMobileNumberVerifyRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class IPChangeMobileNumberVerifyRequestValidator : AbstractValidator<IPChangeMobileNumberVerifyRequest>
    {
        public IPChangeMobileNumberVerifyRequestValidator()
        {
            RuleFor(x => x.OldMobileNumber)
               .Must(new Validation().IsValidString)
               .WithMessage(UserOnbordingMessage.InvalidOldMobile);

            RuleFor(x => x.NewMobileNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(UserOnbordingMessage.InvalidNewMobile);

            RuleFor(x => x.OldOTP)
             .Must(new Validation().IsValidString)
             .WithMessage(UserOnbordingMessage.InvalidOldOTP);

            RuleFor(x => x.NewOTP)
            .Must(new Validation().IsValidString)
            .WithMessage(UserOnbordingMessage.InvalidNewOTP);
        }
    }
}