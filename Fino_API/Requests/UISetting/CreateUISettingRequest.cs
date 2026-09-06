using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.UISetting
{
    public class CreateUISettingRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateUISettingRequestValidator : AbstractValidator<CreateUISettingRequest>
    {
        public CreateUISettingRequestValidator()
        {
            // Key
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage(UISettingMessages.RequiredField)
                .MaximumLength(100).WithMessage(UISettingMessages.InvalidLength);

            // Value
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage(UISettingMessages.RequiredField)
                .MaximumLength(500).WithMessage(UISettingMessages.InvalidLength);

            // Description
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(UISettingMessages.InvalidLength);
        }
    }
}