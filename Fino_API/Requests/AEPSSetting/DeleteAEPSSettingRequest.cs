using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.AEPSSetting
{
    public class DeleteAEPSSettingRequest
    {
        public int AEPSSettingID { get; set; }
    }

    public class DeleteAEPSSettingRequestValidator : AbstractValidator<DeleteAEPSSettingRequest>
    {
        public DeleteAEPSSettingRequestValidator()
        {
            RuleFor(x => x.AEPSSettingID)
                .GreaterThan(0).WithMessage(AEPSSettingMessages.InvalidValue);
        }
    }
}
