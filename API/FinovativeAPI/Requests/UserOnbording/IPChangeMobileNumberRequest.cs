using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserOnbording
{
    public class IPChangeMobileNumberRequest
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

        internal string IsValidModalState()
        {
            var validator = new IPChangeMobileNumberRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class IPChangeMobileNumberRequestValidator : AbstractValidator<IPChangeMobileNumberRequest>
    {
        public IPChangeMobileNumberRequestValidator()
        {
            RuleFor(x => x.OldMobileNumber)
               .Must(new Validation().IsValidString)
               .WithMessage(UserOnbordingMessage.InvalidOldMobile);

            RuleFor(x => x.NewMobileNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(UserOnbordingMessage.InvalidNewMobile);
        }
    }
}