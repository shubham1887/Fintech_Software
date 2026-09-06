using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Sender
{
    public class InsertRequest
    {
        internal int UserID { get; set; }

        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
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

        private string _CityName;

        public string CityName
        {
            get => _CityName.ToTitleCase();
            set => _CityName = value.ToTitleCase();
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

        private string _Pipe;

        public string Pipe
        {
            get => _Pipe.TrimIfNotNull();
            set => _Pipe = value.TrimIfNotNull();
        }

        private string _District;

        public string District
        {
            get => _District.TrimIfNotNull();
            set => _District = value.TrimIfNotNull();
        }

        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }

        private DateTime _FromDate;

        public DateTime FromDate
        {
            get => _FromDate;
            set => _FromDate = value;
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
            RuleFor(x => x.Name)
                .Must(new Validation().IsValidString)
                .MinimumLength(4)
                .WithMessage(SenderMessage.InvalidName);

            RuleFor(x => x.Mobile)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidMobile);

            RuleFor(x => x.UserID)
             .Must(new Validation().IsValidInt)
             .WithMessage(SenderMessage.InvalidUserID);
        }
    }
}