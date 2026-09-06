using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SettingMaster
{
    public class SelectRequest
    {
        private int _SettingMasterID;

        public int SettingMasterID { get => _SettingMasterID; set => _SettingMasterID = value; }

        private bool _IsSecure;
        public bool IsSecure { get => _IsSecure; set => _IsSecure = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.SettingMasterID)
              .Must(new Validation().IsValidInt)
              .WithMessage(SettingMessage.InvalidID);
        }
    }
}