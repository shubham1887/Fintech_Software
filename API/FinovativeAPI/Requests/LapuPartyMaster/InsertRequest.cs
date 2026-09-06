using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.LapuPartyMaster
{
    public class InsertRequest
    {
        private string _PartyName;

        public string PartyName
        {
            get => _PartyName.TrimIfNotNull();
            set => _PartyName = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
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
            RuleFor(x => x.PartyName)
               .Must(new Validation().IsValidString)
               .WithMessage(Message.LapuPartyMasterMessage.InvalidLapuParty);

            RuleFor(x => x.MobileNumber)
              .Must(new Validation().IsValidMobileNumber)
               .WithMessage(Message.LapuPartyMasterMessage.InvalidMobileNumber);
        }
    }
}