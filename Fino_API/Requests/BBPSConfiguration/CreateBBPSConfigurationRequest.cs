using FintechApp.Messages;
using FintechApp.StaticModels;
using FluentValidation;
using FluentValidation.Results;

namespace FintechApp.Requests.BBPSConfiguration
{
    public class CreateBBPSConfigurationRequest
    {
        public int OperatorID { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string PlaceHolder { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string ErrorMeeage { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public int Length { get; set; }
        public string ConfigList { get; set; } = string.Empty;
        public bool IsDisplay { get; set; }

        internal string IsValidModalState()
        {
            var validator = new CreateBBPSConfigurationRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class CreateBBPSConfigurationRequestValidator : AbstractValidator<CreateBBPSConfigurationRequest>
    {
        private static readonly string[] AllowedTypes = { "text", "number", "list", "date" };

        public CreateBBPSConfigurationRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                .GreaterThan(0).WithMessage(BBPSConfigurationMessages.InvalidValue);
            RuleFor(x => x.Label)
                .NotEmpty().WithMessage(BBPSConfigurationMessages.RequiredField)
                .MaximumLength(100).WithMessage(BBPSConfigurationMessages.InvalidLength);
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage(BBPSConfigurationMessages.RequiredField)
                .Must(t => AllowedTypes.Contains(t.ToLower())).WithMessage(BBPSConfigurationMessages.InvalidValue);
            RuleFor(x => x.PlaceHolder)
                .MaximumLength(100).WithMessage(BBPSConfigurationMessages.InvalidLength);
            RuleFor(x => x.ErrorMeeage)
                .MaximumLength(200).WithMessage(BBPSConfigurationMessages.InvalidLength);
            RuleFor(x => x.Sequence)
                .GreaterThan(0).WithMessage(BBPSConfigurationMessages.InvalidValue);
            RuleFor(x => x.Length)
                .GreaterThanOrEqualTo(0).WithMessage(BBPSConfigurationMessages.InvalidValue);
            RuleFor(x => x.ConfigList)
                .MaximumLength(500).WithMessage(BBPSConfigurationMessages.InvalidLength);
        }
    }
}