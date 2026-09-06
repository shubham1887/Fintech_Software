using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UISetting
{
    public class UpdateRequest
    {
        private int _UISettingMasterID;

        public int UISettingMasterID
        {
            get => _UISettingMasterID;
            set => _UISettingMasterID = value;
        }

        private string _Key;

        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }

        private string _Value;

        public string Value
        {
            get => _Value.TrimIfNotNull();
            set => _Value = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.UISettingMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UISettingMessages.InvalidSetting);

            RuleFor(x => x.Key)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UISettingMessages.InvalidKey);

            RuleFor(x => x.Value)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UISettingMessages.InvalidValue);

            RuleFor(x => x.Description)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.UISettingMessages.InvalidDescription);
        }
    }
}