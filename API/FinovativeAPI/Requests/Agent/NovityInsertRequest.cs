using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Agent
{
    public class NovityInsertRequest
    {
        internal int UserID { get; set; }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.ToTitleCase();
            set => _FirmName = value.ToTitleCase();
        }

        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
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

        private string _AgentCode;

        public string AgentCode
        {
            get => _AgentCode.ToTitleCase();
            set => _AgentCode = value.ToTitleCase();
        }

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }
        private string _CityName;

        public string CityName
        {
            get => _CityName.TrimIfNotNull();
            set => _CityName = value.TrimIfNotNull();
        }
        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
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
            var validator = new NovityInsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class NovityInsertRequestValidator : AbstractValidator<NovityInsertRequest>
    {
        public NovityInsertRequestValidator()
        {
            RuleFor(x => x.Name)
                .Must(new Validation().IsValidString)
                .MinimumLength(4)
                .WithMessage(AgentMessage.InvalidName);

            RuleFor(x => x.PAN)
               .Must(new Validation().IsValidPanCard)
               .WithMessage(AgentMessage.InvalidPAN);

            RuleFor(x => x.AadhaarNo)
               .Must(new Validation().IsValidAadharNo)
               .WithMessage(AgentMessage.InvalidAadhar);

            RuleFor(x => x.PinCode)
               .Must(new Validation().IsValidString)
               .WithMessage(AgentMessage.InvalidPincode);

            RuleFor(x => x.UserID)
             .Must(new Validation().IsValidInt)
             .WithMessage(AgentMessage.InvalidUserID);
        }
    }
}