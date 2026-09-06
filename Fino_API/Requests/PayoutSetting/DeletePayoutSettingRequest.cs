using FluentValidation;

namespace FintechApp.Requests.PayoutSetting
{
    public class DeletePayoutSettingRequest { public int PayoutSettingID { get; set; } }
    public class DeletePayoutSettingRequestValidator : AbstractValidator<DeletePayoutSettingRequest>
    {
        public DeletePayoutSettingRequestValidator() { RuleFor(x => x.PayoutSettingID).GreaterThan(0); }
    }
}
