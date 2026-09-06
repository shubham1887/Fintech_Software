using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.UISetting
{
    public class DeleteUISettingRequest
    {
        public int UISettingMasterID { get; set; }
    }

    public class DeleteUISettingRequestValidator : AbstractValidator<DeleteUISettingRequest>
    {
        public DeleteUISettingRequestValidator()
        {
            RuleFor(x => x.UISettingMasterID)
                .GreaterThan(0).WithMessage(UISettingMessages.InvalidValue);
        }
    }
}