using FluentValidation;
namespace FintechApp.Requests.UserWalletMaster
{
    public class UpdateUserWalletMasterRequest { public int UserWalletMasterID { get; set; } public int WalletID { get; set; } public decimal Balance { get; set; } public int UserID { get; set; } public string? BalanceHash { get; set; } }
    public class UpdateUserWalletMasterRequestValidator : AbstractValidator<UpdateUserWalletMasterRequest> { public UpdateUserWalletMasterRequestValidator() { RuleFor(x => x.UserWalletMasterID).GreaterThan(0); } }
}
