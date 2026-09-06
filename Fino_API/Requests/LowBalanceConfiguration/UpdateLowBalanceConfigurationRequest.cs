using FluentValidation;

namespace FintechApp.Requests.LowBalanceConfiguration
{
    public class UpdateLowBalanceConfigurationRequest
    {
        public int LowBalanceConfigurationID { get; set; }
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public string? MobileNumber { get; set; }
        public string? EmailID { get; set; }
        public string? WebHookURL { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateLowBalanceConfigurationRequestValidator : AbstractValidator<UpdateLowBalanceConfigurationRequest>
    {
        public UpdateLowBalanceConfigurationRequestValidator()
        {
            RuleFor(x => x.LowBalanceConfigurationID).GreaterThan(0);
            RuleFor(x => x.UserID).GreaterThan(0);
        }
    }
}
