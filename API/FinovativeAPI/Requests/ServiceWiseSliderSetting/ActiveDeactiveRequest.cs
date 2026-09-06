using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ServiceWiseSliderSetting
{
    public class ActiveDeactiveRequest
    {
        private int _ServiceWiseSliderSettingID;

        public int ServiceWiseSliderSettingID
        {
            get => _ServiceWiseSliderSettingID;
            set => _ServiceWiseSliderSettingID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ActiveDeactiveRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class ActiveDeactiveRequestValidator : AbstractValidator<ActiveDeactiveRequest>
    {
        public ActiveDeactiveRequestValidator()
        {
            RuleFor(x => x.ServiceWiseSliderSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.ServiceWiseSliderSettingMessage.InvalidSlider);
        }
    }
}