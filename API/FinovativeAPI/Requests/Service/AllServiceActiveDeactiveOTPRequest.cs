using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Service
{
    public class AllServiceActiveDeactiveOTPRequest
    {
     
        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }
        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        internal string IsValidModalState()
        {
            var validator = new AllServiceActiveDeactiveOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class AllServiceActiveDeactiveOTPRequestValidator : AbstractValidator<AllServiceActiveDeactiveOTPRequest>
    {
        public AllServiceActiveDeactiveOTPRequestValidator()
        {
        

            RuleFor(x => x.OTP).NotEmpty().WithMessage(Project.Message.UserMessage.InvalidOTP);
        }
    }
}