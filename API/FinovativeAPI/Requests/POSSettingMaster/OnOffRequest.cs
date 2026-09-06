using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.POSSettingMaster
{
    public class OnOffRequest
    {
        private int _POSSettingMasterID;

        public int POSSettingMasterID
        {
            get => _POSSettingMasterID;
            set => _POSSettingMasterID = value;
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
            RuleFor(x => x.POSSettingMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(AEPSMessage.InvalidID);
        }
    }
}