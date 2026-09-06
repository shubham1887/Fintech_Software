using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.eKYC
{
    public class ApiClub_EKYCSendOTPRequest
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

        internal string IsValidModalState()
        {
            var validator = new ApiClub_EKYCSendOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ApiClub_EKYCSendOTPRequestValidator : AbstractValidator<ApiClub_EKYCSendOTPRequest>
    {
        public ApiClub_EKYCSendOTPRequestValidator()
        {
            RuleFor(x => x.PanNo)
                    .Must(new Validation().IsValidPanCard)
                    .WithMessage(Message.UserMessage.InvaildPanNumber);

            RuleFor(x => x.AadhaarNo)
            .Must(new Validation().IsValidAadharNo)
            .WithMessage(Message.UserMessage.IsValidAadharNo);

            RuleFor(x => x.MobileNo)
                     .Must(new Validation().IsValidMobileNumber)
                    .WithMessage(AEPSMessage.InValidConsumerNumber);
        }
    }
}