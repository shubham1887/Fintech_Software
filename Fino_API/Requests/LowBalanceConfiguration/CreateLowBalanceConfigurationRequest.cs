using FluentValidation;

namespace FintechApp.Requests.LowBalanceConfiguration
{
    public class CreateLowBalanceConfigurationRequest
    {
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public string? MobileNumber { get; set; }
        public string? EmailID { get; set; }
        public string? WebHookURL { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateLowBalanceConfigurationRequestValidator : AbstractValidator<CreateLowBalanceConfigurationRequest>
    {
        public CreateLowBalanceConfigurationRequestValidator()
        {
            RuleFor(x => x.UserID).GreaterThan(0);
            RuleFor(x => x.WalletID).GreaterThan(0);
        }
    }
}
