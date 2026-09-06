using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class FingPay_EKYCSendOTPRequest
    {
        private string _AadhaarNo;

        public string AadhaarNo
        {
            get => _AadhaarNo.TrimIfNotNull();
            set => _AadhaarNo = value.TrimIfNotNull();
        }

        private string _PanNo;

        public string PanNo
        {
            get => _PanNo.TrimIfNotNull();
            set => _PanNo = value.TrimIfNotNull();
        }

        private string _MobileNo;

        public string MobileNo
        {
            get => _MobileNo.TrimIfNotNull();
            set => _MobileNo = value.TrimIfNotNull();
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
            var validator = new FingPay_EKYCSendOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class FingPay_EKYCSendOTPRequestValidator : AbstractValidator<FingPay_EKYCSendOTPRequest>
    {
        public FingPay_EKYCSendOTPRequestValidator()
        {
            RuleFor(x => x.PanNo)
                      .Must(new Validation().IsValidPanCard)
                      .WithMessage(Message.UserMessage.InvaildPanNumber);

            RuleFor(x => x.AadhaarNo)
          .Must(new Validation().IsValidAadharNo)
          .WithMessage(Message.UserMessage.InvalidAadhar);

            RuleFor(x => x.MobileNo)
       .Must(new Validation().IsValidMobileNumber)
       .WithMessage(Message.UserMessage.InvalidMobileNumber);
        }
    }
}