using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class TwoFactorAuthenticationRequest
    {
        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint.TrimIfNotNull();
            set => _FingerPrint = value.TrimIfNotNull();
        }
        private int _OperatorID;
        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _Latitude;
        public string Latitude
        {
            get => _Latitude.TrimIfNotNull();
            set => _Latitude = value.TrimIfNotNull();
        }
        private string _Longitude;
        public string Longitude
        {
            get => _Longitude.TrimIfNotNull();
            set => _Longitude = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new TwoFAAuthenticationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class TwoFAAuthenticationRequestValidator : AbstractValidator<TwoFactorAuthenticationRequest>
    {
        public TwoFAAuthenticationRequestValidator()
        {
            RuleFor(x => x.FingerPrint)
                    .Must(m => new Validation().IsValidStringWithMinimumLength(m, length: 20))
                    .WithMessage(AEPSMessage.InvalidFingerPrint);
        }
    }
}