using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class LoginOTPActivateRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private bool _IsLoginOTP;
        public bool IsLoginOTP { get => _IsLoginOTP; set => _IsLoginOTP = value; }

        internal string IsValidModalState()
        {
            var validator = new LoginOTPActivateRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class LoginOTPActivateRequestValidator : AbstractValidator<LoginOTPActivateRequest>
    {
        public LoginOTPActivateRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserMessage.InvalidUser);
        }
    }
}