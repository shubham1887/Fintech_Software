using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PinGeneration
{
    public class PinGenerationRequest
    {

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private string _Password;

        public string Password
        {
            get => _Password.TrimIfNotNull();
            set => _Password = value.TrimIfNotNull();
        }

        private int _Pin;

        public int Pin
        {
            get => _Pin;
            set => _Pin = value;
        }
        
        private string _DeviceID;

        public string DeviceID
        {
            get => _DeviceID.TrimIfNotNull();
            set => _DeviceID = value.TrimIfNotNull();
        }
        private string _Device;

        public string Device
        {
            get => _Device.TrimIfNotNull();
            set => _Device = value.TrimIfNotNull();
        }

        private int _ConfirmPin;

        public int ConfirmPin
        {
            get => _ConfirmPin;
            set => _ConfirmPin = value;
        }
        internal string IsValidModalState()
        {
            var validator = new PinGenerationRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class PinGenerationRequestValidator : AbstractValidator<PinGenerationRequest>
    {
        public PinGenerationRequestValidator()
        {
            RuleFor(x => x.Pin)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.UserMessage.InvalidPin);
            
            RuleFor(x => x.ConfirmPin)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.UserMessage.InvalidConfirmPin);
        }
    }
}