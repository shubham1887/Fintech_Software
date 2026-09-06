using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Poject.Requests.CyberPlate
{
    public class SelectByIDRequest
    {
        private int _CyberPlateSettingID;

        public int CyberPlateSettingID
        {
            get => _CyberPlateSettingID;
            set => _CyberPlateSettingID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.CyberPlateSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(CyberPlateSettingMessage.InvalidID);
        }
    }
}