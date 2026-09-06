using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS.InstantPayAEPS
{
    public class InstantPay_MerchantOnboardingRequest
    {
        private string _Merchantmobilenumber;

        public string Merchantmobilenumber
        {
            get => _Merchantmobilenumber.TrimIfNotNull();
            set => _Merchantmobilenumber = value.TrimIfNotNull();
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

        private string _Aadhar;

        public string Aadhar
        {
            get => _Aadhar?.TrimIfNotNull();
            set => _Aadhar = value?.TrimIfNotNull();
        }

        private string _Email;

        public string Email
        {
            get => _Email?.TrimIfNotNull();
            set => _Email = value?.TrimIfNotNull();
        }

        private string _DOB;

        public string DOB
        {
            get => _DOB?.TrimIfNotNull();
            set => _DOB = value?.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City?.TrimIfNotNull();
            set => _City = value?.TrimIfNotNull();
        }

        private string _FullAddress;

        public string FullAddress
        {
            get => _FullAddress?.TrimIfNotNull();
            set => _FullAddress = value?.TrimIfNotNull();
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber?.TrimIfNotNull();
            set => _PanNumber = value?.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode?.TrimIfNotNull();
            set => _Pincode = value?.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InstantPay_MerchantOnboardingRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InstantPay_MerchantOnboardingRequestValidator : AbstractValidator<InstantPay_MerchantOnboardingRequest>
    {
        public InstantPay_MerchantOnboardingRequestValidator()
        {
            RuleFor(x => x.Merchantmobilenumber)
                      .Must(new Validation().IsValidMobileNumber)
                      .WithMessage(AEPSMessage.InValidConsumerNumber);

            RuleFor(x => x.Latitude)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AEPSMessage.InvalidLatitude);

            RuleFor(x => x.Longitude)
                      .Must(new Validation().IsValidString)
                      .WithMessage(AEPSMessage.InvalidLongitude);
        }
    }
}