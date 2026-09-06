using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS.YesBankAEPS
{
    public class YesBank_AuthenticationRequest
    {
        private string _AadhaarNumber;

        public string AadhaarNumber
        {
            get => _AadhaarNumber.TrimIfNotNull();
            set => _AadhaarNumber = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }


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

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private string _IPAddess;

        public string IPAddess
        {
            get => _IPAddess.TrimIfNotNull();
            set => _IPAddess = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new YesBank_AuthenticationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class YesBank_AuthenticationRequestValidator : AbstractValidator<YesBank_AuthenticationRequest>
    {
        public YesBank_AuthenticationRequestValidator()
        {

            RuleFor(x => x.AadhaarNumber)
                      .Must(new Validation().IsValidAadharNo)
                      .WithMessage(AEPSMessage.InvalidAadhaarNumber);

            RuleFor(x => x.FingerPrint)
                    .Must(m => new Validation().IsValidStringWithMinimumLength(m, length: 20))
                    .WithMessage(AEPSMessage.InvalidFingerPrint);


            RuleFor(x => x.Latitude)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AEPSMessage.InvalidLatitude);

            RuleFor(x => x.Longitude)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AEPSMessage.InvalidLongitude);
        }
    }
}