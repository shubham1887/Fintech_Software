using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.EmailSetting
{
    public class SelectByIdRequest
    {
        private int _EmailSettingMasterID;

        public int EmailSettingMasterID
        {
            get => _EmailSettingMasterID;
            set => _EmailSettingMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIdRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIdRequestValidator : AbstractValidator<SelectByIdRequest>
    {
        public SelectByIdRequestValidator()
        {
            RuleFor(x => x.EmailSettingMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.EmailSettingMessage.InvalidEmail);
        }
    }
}