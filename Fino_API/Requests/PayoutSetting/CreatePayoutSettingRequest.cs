using FluentValidation;

namespace FintechApp.Requests.PayoutSetting
{
    public class CreatePayoutSettingRequest
    {
        public string Name { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Slat { get; set; } = string.Empty;
        public string ActionURL { get; set; } = string.Empty;
        public string? StatusCheckURL { get; set; }
        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public string? Mode { get; set; }
    }
    public class CreatePayoutSettingRequestValidator : AbstractValidator<CreatePayoutSettingRequest>
    {
        public CreatePayoutSettingRequestValidator() { RuleFor(x => x.Name).NotEmpty(); RuleFor(x => x.ActionURL).NotEmpty(); }
    }
}
