using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class ChangePasswordAfterExpiryRequest
    {
        private string _OldPassword;

        public string OldPassword
        {
            get => _OldPassword.TrimIfNotNull();
            set => _OldPassword = value.TrimIfNotNull();
        }

        private string _NewPassword;

        public string NewPassword
        {
            get => _NewPassword.TrimIfNotNull();
            set => _NewPassword = value.TrimIfNotNull();
        }

        private string _ConfirmPassword;

        public string ConfirmPassword
        {
            get => _ConfirmPassword.TrimIfNotNull();
            set => _ConfirmPassword = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber;
            set => _MobileNumber = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ChangePasswordAfterExpiryValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }

            if (NewPassword == OldPassword)
                return UserMessage.ShouldDifferentNewPassword;

            if (!new Validation().IsComplexPassword(NewPassword))
            {
                return UserMessage.ComplexPass;
            }

            if (NewPassword != ConfirmPassword)
                return UserMessage.InvalidConfirmPass;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ChangePasswordAfterExpiryValidator : AbstractValidator<ChangePasswordAfterExpiryRequest>
    {
        public ChangePasswordAfterExpiryValidator()
        {
            RuleFor(x => x.MobileNumber)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidMobileNumber);

            RuleFor(x => x.OldPassword)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidOldPass);

            RuleFor(x => x.NewPassword)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidNewPass);

            RuleFor(x => x.ConfirmPassword)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidConfirmPass);
        }
    }
}