using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS.CredoPay
{
    public class CredoPay_AuthenticationRequest
    {
        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint.TrimIfNotNull();
            set => _FingerPrint = value.TrimIfNotNull();
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
        private string _IPAddess;
        public string IPAddess
        {
            get => _IPAddess.TrimIfNotNull();
            set => _IPAddess = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new CredoPay_AuthenticationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class CredoPay_AuthenticationRequestValidator : AbstractValidator<CredoPay_AuthenticationRequest>
    {
        public CredoPay_AuthenticationRequestValidator()
        {
            RuleFor(x => x.FingerPrint)
               .Must(new Validation().IsValidString)
               .WithMessage(AEPSMessage.InvalidFingerPrint);

        }
    }
}
