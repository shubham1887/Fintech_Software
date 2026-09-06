using FluentValidation;

namespace FintechApp.Requests.PayoutSetting
{
    public class GetPayoutSettingRequest { public int PayoutSettingID { get; set; } }
    public class GetPayoutSettingRequestValidator : AbstractValidator<GetPayoutSettingRequest>
    {
        public GetPayoutSettingRequestValidator() { RuleFor(x => x.PayoutSettingID).GreaterThan(0); }
    }
}
