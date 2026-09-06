using FintechApp.Messages;
using FintechApp.StaticModels;
using FluentValidation;
using FluentValidation.Results;

namespace FintechApp.Requests.BBPSConfiguration
{
    public class UpdateBBPSConfigurationRequest
    {
        public int BBPSConfigurationID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;

        internal string IsValidModalState()
        {
            var validator = new UpdateBBPSConfigurationRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class UpdateBBPSConfigurationRequestValidator : AbstractValidator<UpdateBBPSConfigurationRequest>
    {
        public UpdateBBPSConfigurationRequestValidator()
        {
            RuleFor(x => x.BBPSConfigurationID)
                .GreaterThan(0).WithMessage(BBPSConfigurationMessages.InvalidValue);
            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(BBPSConfigurationMessages.RequiredField)
                .MaximumLength(100).WithMessage(BBPSConfigurationMessages.InvalidLength);
            RuleFor(x => x.OldValue)
                .NotEmpty().WithMessage(BBPSConfigurationMessages.RequiredField)
                .MaximumLength(500).WithMessage(BBPSConfigurationMessages.InvalidLength);
            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(BBPSConfigurationMessages.RequiredField)
                .MaximumLength(500).WithMessage(BBPSConfigurationMessages.InvalidLength);
        }
    }
}