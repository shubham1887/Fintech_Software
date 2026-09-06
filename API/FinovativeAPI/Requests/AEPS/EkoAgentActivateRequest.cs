using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class EkoAgentActivateRequest
    {
        public CustomFile PanCard = new CustomFile();

        public CustomFile AadharFront = new CustomFile();

        public CustomFile AadharBack = new CustomFile();

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

        private string _Devicenumber;

        public string Devicenumber
        {
            get => _Devicenumber.TrimIfNotNull();
            set => _Devicenumber = value.TrimIfNotNull();
        }

        private string _Modelname;

        public string Modelname
        {
            get => _Modelname.TrimIfNotNull();
            set => _Modelname = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new EkoAgentActivateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class EkoAgentActivateRequestValidator : AbstractValidator<EkoAgentActivateRequest>
    {
        public EkoAgentActivateRequestValidator()
        {
            RuleFor(x => x.Modelname)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidModelname);

            RuleFor(x => x.Devicenumber)
          .Must(new Validation().IsValidString)
          .WithMessage(AEPSMessage.InvalidDevicenumber);

            RuleFor(x => x.PinCode)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidPinCode);

            RuleFor(x => x.Address)
           .Must(new Validation().IsValidString)
           .WithMessage(AEPSMessage.InvalidAddress);

            RuleFor(x => x.City)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidCity);
        }
    }
}