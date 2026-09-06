using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.BusSettingMaster
{
    public class OnOffRequest
    {
        private int _BusSettingMasterID;

        public int BusSettingMasterID
        {
            get => _BusSettingMasterID;
            set => _BusSettingMasterID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        internal string IsValidModalState()
        {
            var validator = new OnOffRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class OnOffRequestValidator : AbstractValidator<OnOffRequest>
    {
        public OnOffRequestValidator()
        {
            RuleFor(x => x.BusSettingMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(BusSettingMasterMessages.InvalidID);
        }
    }
}