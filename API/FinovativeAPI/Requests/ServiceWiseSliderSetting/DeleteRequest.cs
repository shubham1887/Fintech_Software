using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.Requests.ServiceWiseSliderSetting
{
    public class DeleteRequest
    {
        private int _ServiceWiseSliderSettingID;

        public int ServiceWiseSliderSettingID
        {
            get => _ServiceWiseSliderSettingID;
            set => _ServiceWiseSliderSettingID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures.FirstOrDefault().ToString();
        }
    }

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.ServiceWiseSliderSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.ServiceWiseSliderSettingMessage.InvalidSlider);
        }
    }
}