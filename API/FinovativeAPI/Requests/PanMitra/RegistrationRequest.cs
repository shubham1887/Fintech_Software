using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PanMitra
{
    public class RegistrationRequest
    {
        private string _Name;
        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }
        private string _MobileNumber;
        public string MobileNumber
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }
        private string _VleID;
        public string VleID
        {
            get => _VleID.TrimIfNotNull();
            set => _VleID = value.TrimIfNotNull();
        }
        private string _EmailID;
        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }
        private string _FirmName;
        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
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
        private string _Pincode;
        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }
        private string _PANNo;
        public string PANNo
        {
            get => _PANNo.TrimIfNotNull();
            set => _PANNo = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new RegistrationRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
    {
        public RegistrationRequestValidator()
        {
            RuleFor(x => x.FirmName)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidFirmName);

            RuleFor(x => x.MobileNumber)
                .Must(new Validation().IsValidMobileNumber)
                .WithMessage(Message.UserMessage.InvalidMobileNumber);

            RuleFor(x => x.EmailID)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidEmailID);

            RuleFor(x => x.State)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidState);
            RuleFor(x => x.Address)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidAddress);
            RuleFor(x => x.Pincode)
               .Must(new Validation().IsValidString)
               .WithMessage(Message.UserMessage.InvalidPincode);

            RuleFor(x => x.PANNo)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.UserMessage.InvalidPan);

            
        }
    }
}

