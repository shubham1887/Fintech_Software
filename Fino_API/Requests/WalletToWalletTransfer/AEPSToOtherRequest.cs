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
    /// <summary>
    /// Move money between two of the caller's OWN wallets, in either direction — the general form
    /// of <see cref="AEPSToOtherRequest"/>, which could only ever move money out of AEPS. Both ids
    /// are validated against the wallets the caller actually holds, so a wallet id the user does
    /// not own is refused rather than transferred from.
    /// </summary>
    public class WalletTransferRequest
    {
        public int FromWalletID { get; set; }
        public int ToWalletID { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; } = string.Empty;
    }

    public class WalletTransferRequestValidator : AbstractValidator<WalletTransferRequest>
    {
        public WalletTransferRequestValidator()
        {
            RuleFor(x => x.FromWalletID).GreaterThan(0);
            RuleFor(x => x.ToWalletID).GreaterThan(0);
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }

}
