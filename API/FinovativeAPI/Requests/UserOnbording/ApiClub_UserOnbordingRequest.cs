using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserOnbording
{
    public class ApiClub_UserOnbordingRequest
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

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        public CustomFile PanCard = new CustomFile();

        public CustomFile CancellationCheque = new CustomFile();

        internal string IsValidModalState()
        {
            var validator = new ApiClub_UserOnbordingRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ApiClub_UserOnbordingRequestValidator : AbstractValidator<ApiClub_UserOnbordingRequest>
    {
        public ApiClub_UserOnbordingRequestValidator()
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