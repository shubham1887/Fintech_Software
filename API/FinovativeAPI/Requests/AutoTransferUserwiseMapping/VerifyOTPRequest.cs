using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AutoTransferUserwiseMapping
{
    public class VerifyOTPRequest
    {
        private string _OTP;

        public string OTP
        {
            get => _OTP;
            set => _OTP = value;
        }

        private int _SelectedUserID;

        public int SelectedUserID
        {
            get => _SelectedUserID;
            set => _SelectedUserID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new VerifyOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class VerifyOTPRequestValidator : AbstractValidator<VerifyOTPRequest>
    {
        public VerifyOTPRequestValidator()
        {
            RuleFor(x => x.OTP)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AutoTransferMessages.InValidOTP);

            RuleFor(x => x.SelectedUserID)
                      .Must(new Validation().IsValidpositive)
                      .WithMessage(AutoTransferMessages.InValidUser);
        }
    }
}