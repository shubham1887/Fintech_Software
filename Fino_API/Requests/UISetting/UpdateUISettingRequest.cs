using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.UISetting
{
    public class UpdateUISettingRequest
    {
        public int UISettingMasterID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateUISettingRequestValidator : AbstractValidator<UpdateUISettingRequest>
    {
        public UpdateUISettingRequestValidator()
        {
            // UISettingMasterID
            RuleFor(x => x.UISettingMasterID)
                .GreaterThan(0).WithMessage(UISettingMessages.InvalidValue);

            // FieldName
            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(UISettingMessages.RequiredField)
                .MaximumLength(100).WithMessage(UISettingMessages.InvalidLength);

            // OldValue
            RuleFor(x => x.OldValue)
                .NotEmpty().WithMessage(UISettingMessages.RequiredField)
                .MaximumLength(500).WithMessage(UISettingMessages.InvalidLength);

            // NewValue
            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(UISettingMessages.RequiredField)
                .MaximumLength(500).WithMessage(UISettingMessages.InvalidLength);
        }
    }
}