using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.ProviderMaster
{
    public class CreateProviderMasterRequest
    {
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderUrl { get; set; } = string.Empty;
        public string ProviderUniqueID { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsHLR { get; set; }
        public decimal RechargeBalance { get; set; }
        public decimal DMTBalance { get; set; }
        public int ResponseGroupID { get; set; }
        public bool IsRealAPI { get; set; }
        public string LoginID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string SupportNumber { get; set; } = string.Empty;
        public int LowBalanceAmount { get; set; }
    }

    public class CreateProviderMasterRequestValidator : AbstractValidator<CreateProviderMasterRequest>
    {
        public CreateProviderMasterRequestValidator()
        {
            RuleFor(x => x.ProviderName)
                .NotEmpty().WithMessage(ProviderMasterMessages.InvalidName)
                .MaximumLength(150).WithMessage(ProviderMasterMessages.InvalidLength);

            RuleFor(x => x.ProviderUrl)
                .NotEmpty().WithMessage(ProviderMasterMessages.InvalidUrl);

            RuleFor(x => x.ProviderUniqueID)
                .MaximumLength(150).WithMessage(ProviderMasterMessages.InvalidLength);

            RuleFor(x => x.LoginID)
                .MaximumLength(200).WithMessage(ProviderMasterMessages.InvalidLength);

            RuleFor(x => x.Token)
                .MaximumLength(200).WithMessage(ProviderMasterMessages.InvalidLength);

            RuleFor(x => x.SupportNumber)
                .MaximumLength(10).WithMessage(ProviderMasterMessages.InvalidLength);
        }
    }
}
