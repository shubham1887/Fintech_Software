using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Agent
{
    public class InsertRequest
    {
        internal int UserID { get; set; }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.ToTitleCase();
            set => _FirmName = value.ToTitleCase();
        }

        private string _FirstName;

        public string FirstName
        {
            get => _FirstName.ToTitleCase();
            set => _FirstName = value.ToTitleCase();
        }

        private string _AadhaarNo;

        public string AadhaarNo
        {
            get => _AadhaarNo.ToTitleCase();
            set => _AadhaarNo = value.ToTitleCase();
        }

        private string _PAN;

        public string PAN
        {
            get => _PAN.TrimIfNotNull();
            set => _PAN = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.ToTitleCase();
            set => _Address = value.ToTitleCase();
        }

        private string _AgentCode;

        public string AgentCode
        {
            get => _AgentCode.ToTitleCase();
            set => _AgentCode = value.ToTitleCase();
        }

        private string _LastName;

        public string LastName
        {
            get => _LastName.ToTitleCase();
            set => _LastName = value.ToTitleCase();
        }

        private string _DMTName;

        [DefaultValue("")]
        public string DMTName
        {
            get => _DMTName.TrimIfNotNull();
            set => _DMTName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .Must(new Validation().IsValidString)
                .MinimumLength(4)
                .WithMessage(AgentMessage.InvalidName);

            //  RuleFor(x => x.FirmName)
            //.Must(new Validation().IsValidString)
            //.MinimumLength(4)
            //.WithMessage(AgentMessage.InvalidFirmName);

            RuleFor(x => x.PAN)
               .Must(new Validation().IsValidPanCard)
               .WithMessage(AgentMessage.InvalidPAN);

            RuleFor(x => x.AadhaarNo)
               .Must(new Validation().IsValidAadharNo)
               .WithMessage(AgentMessage.InvalidAadhar);

            RuleFor(x => x.Address)
               .Must(new Validation().IsValidString)
               .WithMessage(AgentMessage.InvalidAddress);

            RuleFor(x => x.City)
               .Must(new Validation().IsValidString)
               .WithMessage(AgentMessage.InvalidCity);

            RuleFor(x => x.PinCode)
               .Must(new Validation().IsValidString)
               .WithMessage(AgentMessage.InvalidPincode);

            RuleFor(x => x.UserID)
             .Must(new Validation().IsValidInt)
             .WithMessage(AgentMessage.InvalidUserID);
        }
    }
}