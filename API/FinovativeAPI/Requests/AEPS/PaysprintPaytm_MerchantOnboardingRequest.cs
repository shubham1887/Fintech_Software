using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class PaysprintPaytm_MerchantOnboardingRequest
    {

        private string _MerchantName;

        public string MerchantName
        {
            get => _MerchantName?.TrimIfNotNull();
            set => _MerchantName = value?.TrimIfNotNull();
        }
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

        public string  Longitude
        {
            get => _Longitude?.TrimIfNotNull();
            set => _Longitude = value?.TrimIfNotNull();
        }

        private string _Statecode;

        public string Statecode
        {
            get => _Statecode?.TrimIfNotNull();
            set => _Statecode = value?.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City?.TrimIfNotNull();
            set => _City = value?.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address?.TrimIfNotNull();
            set => _Address = value?.TrimIfNotNull();
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
        private string _subMerchantId;

        public string subMerchantId
        {
            get => _subMerchantId?.TrimIfNotNull();
            set => _subMerchantId = value?.TrimIfNotNull();
        }




        internal string IsValidModalState()
        {
            var validator = new PaysprintPaytmMerchantOnboardingRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class PaysprintPaytmMerchantOnboardingRequestValidator : AbstractValidator<PaysprintPaytm_MerchantOnboardingRequest>
    {
        public PaysprintPaytmMerchantOnboardingRequestValidator()
        {

            RuleFor(x => x.Merchantmobilenumber)
                      .Must(new Validation().IsValidMobileNumber)
                      .WithMessage(AEPSMessage.InValidConsumerNumber);
        }
    }
}