using FluentValidation;

namespace FintechApp.Requests.WalletToWalletTransfer
{
    public class AEPSToOtherRequest
    {
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; } = string.Empty;
    }

    public class AEPSToOtherRequestValidator : AbstractValidator<AEPSToOtherRequest>
    {
        public AEPSToOtherRequestValidator()
        {
            RuleFor(x => x.WalletID).GreaterThan(0);
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }
}
