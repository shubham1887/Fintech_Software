using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.Wallet
{
    // Field-level wallet update (mirrors UpdateServiceRequest). Admin may only change the
    // wallet Name or toggle IsActive (enable/disable) — never create or delete a wallet.
    public class UpdateWalletRequest
    {
        public int WalletID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateWalletRequestValidator : AbstractValidator<UpdateWalletRequest>
    {
        public UpdateWalletRequestValidator()
        {
            RuleFor(x => x.WalletID).GreaterThan(0).WithMessage(WalletMessages.NotFound);
            RuleFor(x => x.FieldName).NotEmpty().WithMessage(WalletMessages.NameRequired);
        }
    }
}
