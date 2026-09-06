using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class PaysprintPaytm_DailyAuthenticationRequest
    {
        private string _Accessmodetype;

        public string Accessmodetype
        {
            get => _Accessmodetype.TrimIfNotNull();
            set => _Accessmodetype = value.TrimIfNotNull();
        }

        private string _adhaarnumber;

        public string adhaarnumber
        {
            get => _adhaarnumber;
            set => _adhaarnumber = value;
        }

        private string _mobilenumber;

        public string mobilenumber
        {
            get => _mobilenumber;
            set => _mobilenumber = value;
        }

        private string _Latitude;

        public string Latitude
        {
            get => _Latitude?.TrimIfNotNull();
            set => _Latitude = value?.TrimIfNotNull();
        }

        private string _Longitude;

        public string Longitude
        {
            get => _Longitude?.TrimIfNotNull();
            set => _Longitude = value?.TrimIfNotNull();
        }

        private string _referenceno;

        public string referenceno
        {
            get => _referenceno.TrimIfNotNull();
            set => _referenceno = value.TrimIfNotNull();
        }

        private string _ipaddress;

        public string ipaddress
        {
            get => _ipaddress.TrimIfNotNull();
            set => _ipaddress = value.TrimIfNotNull();
        }
        public string fingerprint { get; set; }

        public string timestamp { get; set; }

        public string submerchantid { get; set; }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new PaysprintPaytm_DailyAuthenticationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class PaysprintPaytm_DailyAuthenticationRequestValidator : AbstractValidator<PaysprintPaytm_DailyAuthenticationRequest>
    {
        public PaysprintPaytm_DailyAuthenticationRequestValidator()
        {
            RuleFor(x => x.adhaarnumber)
               .Must(new Validation().IsValidString)
               .WithMessage(AEPSMessage.InvalidAadhaarNumber);

            RuleFor(x => x.Accessmodetype)
                    .Must(new Validation().IsValidString)
                    .WithMessage(AEPSMessage.InvalidAccessmodetype);
        }
    }
}