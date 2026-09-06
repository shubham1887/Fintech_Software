using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.IO.Pipelines;

namespace Project.Requests.DailyAuthentication
{
    public class Paysprint_AuthenticationManualRequest
    {
        private string _accessmodetype;

        public string accessmodetype
        {
            get => _accessmodetype.TrimIfNotNull();
            set => _accessmodetype = value.TrimIfNotNull();
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

        private string _Pipe;
        public string Pipe
        {
            get => _Pipe;
            set => _Pipe = value;
        }
        private decimal _Latitude;
        public decimal Latitude
        {
            get => _Latitude;
            set => _Latitude = value;
        }
        private decimal _Longitude;
        public decimal Longitude
        {
            get => _Longitude;
            set => _Longitude = value;
        }

        internal string IsValidModalState()
        {
            var validator = new Paysprint_AuthenticationManualRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Paysprint_AuthenticationManualRequestValidator : AbstractValidator<Paysprint_AuthenticationManualRequest>
    {
        public Paysprint_AuthenticationManualRequestValidator()
        {
            RuleFor(x => x.adhaarnumber)
               .Must(new Validation().IsValidString)
               .WithMessage(AEPSMessage.InvalidProfileAadhaarNumber);
            RuleFor(x => x.Pipe)
              .Must(new Validation().IsValidString)
              .WithMessage(AEPSMessage.InvalidIIN);
        }
    }
}


