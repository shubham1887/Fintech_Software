using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SettingMaster
{
    public class UpdateRequest
    {
        private int _SettingMasterID;

        public int SettingMasterID
        {
            get => _SettingMasterID;
            set => _SettingMasterID = value;
        }

        private string _Key;

        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }

        private string _Values;

        public string Values
        {
            get => _Values.TrimIfNotNull();
            set => _Values = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new OperatorSelectByServiceIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class OperatorSelectByServiceIDRequestValidator : AbstractValidator<UpdateRequest>
    {
        public OperatorSelectByServiceIDRequestValidator()
        {
            RuleFor(x => x.SettingMasterID)
              .Must(new Validation().IsValidInt)
              .WithMessage(SettingMessage.InvalidID);
            RuleFor(x => x.Key)
              .Must(new Validation().IsValidString)
              .WithMessage(SettingMessage.InvalidKey);
            RuleFor(x => x.Key)
             .Must(new Validation().IsValidString)
             .WithMessage(SettingMessage.InvalidValue);
        }
    }
}