using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.EcollectionSetting
{
    public class SelectByIDRequest
    {
        private int _EcollectionSettingID;

        public int EcollectionSettingID
        {
            get => _EcollectionSettingID;
            set => _EcollectionSettingID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(config => config.EcollectionSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(EcollectionMessage.InvalidID);
        }
    }
}