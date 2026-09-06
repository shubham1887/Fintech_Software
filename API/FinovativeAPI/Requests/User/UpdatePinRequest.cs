using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class UpdatePinRequest
    {
        private int _NewPin;

        public int NewPin
        {
            get => _NewPin;
            set => _NewPin = value;
        }

        private int _ConfirmPin;

        public int ConfirmPin
        {
            get => _ConfirmPin;
            set => _ConfirmPin = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _CurrentPassword;

        public string CurrentPassword
        {
            get => _CurrentPassword;
            set => _CurrentPassword = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdatePinRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }

            //if (OldPin.Equals(NewPin))
            //{
            //    return "Need different new pin";
            //}

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdatePinRequestValidator : AbstractValidator<UpdatePinRequest>
    {
        public UpdatePinRequestValidator()
        {
            //RuleFor(x => x.OldPin)
            //    .Must(new Validation().IsValidInt)
            //    .WithMessage(Message.UserMessage.InvalidOldPin);

            RuleFor(x => x.CurrentPassword)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UserMessage.InvalidPassword);

            RuleFor(x => x.NewPin)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidNewPin);

            RuleFor(x => x.ConfirmPin)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidConfirmPin);
        }
    }
}