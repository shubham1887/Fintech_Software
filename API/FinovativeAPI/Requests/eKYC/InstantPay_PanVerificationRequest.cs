using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;


namespace Project.Requests.eKYC
{
    public class InstantPay_PanVerificationRequest
    {
        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InstantPay_PanVerificationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal class InstantPay_PanVerificationRequestValidator : AbstractValidator<InstantPay_PanVerificationRequest>
        {
            public InstantPay_PanVerificationRequestValidator()
            {
                RuleFor(x => x.PanNumber)
               .Must(new Validation().IsValidString)
               .WithMessage(Message.eKYCMessage.InvalidPAN);
            }
        }
    }
}
