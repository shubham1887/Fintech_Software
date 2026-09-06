using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.AEPSSetting
{
    public class CreateAEPSSettingRequest
    {
        public string Name { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Slat { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string Mode { get; set; } = string.Empty;
        public bool IsMerchantVerify { get; set; }
    }

    public class CreateAEPSSettingRequestValidator : AbstractValidator<CreateAEPSSettingRequest>
    {
        public CreateAEPSSettingRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(AEPSSettingMessages.RequiredField)
                .MaximumLength(100).WithMessage(AEPSSettingMessages.InvalidLength);

            RuleFor(x => x.MerchantID)
                .NotEmpty().WithMessage(AEPSSettingMessages.InvalidMerchantID)
                .MaximumLength(100).WithMessage(AEPSSettingMessages.InvalidLength);

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(AEPSSettingMessages.InvalidToken)
                .MaximumLength(200).WithMessage(AEPSSettingMessages.InvalidLength);

            RuleFor(x => x.Slat)
                .NotEmpty().WithMessage(AEPSSettingMessages.RequiredField)
                .MaximumLength(200).WithMessage(AEPSSettingMessages.InvalidLength);

            RuleFor(x => x.Mode)
                .Must(m => string.IsNullOrEmpty(m) || m == "LIVE" || m == "TEST")
                .WithMessage(AEPSSettingMessages.InvalidMode);

            RuleFor(x => x.MinimumAmount)
                .GreaterThanOrEqualTo(0).WithMessage(AEPSSettingMessages.InvalidAmount);

            RuleFor(x => x.MaximumAmount)
                .GreaterThan(0).WithMessage(AEPSSettingMessages.InvalidAmount);

            RuleFor(x => x)
                .Must(x => x.MinimumAmount < x.MaximumAmount)
                .WithMessage(AEPSSettingMessages.InvalidAmount);
        }
    }
}
