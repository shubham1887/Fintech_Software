using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.DMTSetting
{
    public class SelectTransferModeRequest
    {
        private string _ActiveDMT;

        public string ActiveDMT
        {
            get => _ActiveDMT.TrimIfNotNull();
            set => _ActiveDMT = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectTransferModeRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectTransferModeRequestValidator : AbstractValidator<SelectTransferModeRequest>
    {
        public SelectTransferModeRequestValidator()
        {
            RuleFor(x => x.ActiveDMT)
                .Must(new Validation().IsValidString)
                .WithMessage(DMTSettingMessages.InvalidActiveDMT);
        }
    }
}