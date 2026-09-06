using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.AEPSSetting
{
    public class UpdateAEPSSettingRequest
    {
        public int AEPSSettingID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateAEPSSettingRequestValidator : AbstractValidator<UpdateAEPSSettingRequest>
    {
        public UpdateAEPSSettingRequestValidator()
        {
            RuleFor(x => x.AEPSSettingID)
                .GreaterThan(0).WithMessage(AEPSSettingMessages.InvalidValue);

            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(AEPSSettingMessages.RequiredField);

            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(AEPSSettingMessages.RequiredField);
        }
    }
}
