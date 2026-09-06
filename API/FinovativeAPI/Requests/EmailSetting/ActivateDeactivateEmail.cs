using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.EmailSetting
{
    public class ActivateDeactivateEmail
    {
        private int _EmailSettingMasterID;

        public int EmailSettingMasterID
        {
            get => _EmailSettingMasterID;
            set => _EmailSettingMasterID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = false;
        }

        internal string IsValidModalState()
        {
            var validator = new ActivateDeactivateEmailValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ActivateDeactivateEmailValidator : AbstractValidator<ActivateDeactivateEmail>
    {
        public ActivateDeactivateEmailValidator()
        {
            RuleFor(x => x.EmailSettingMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.EmailSettingMessage.InvalidEmail);
        }
    }
}