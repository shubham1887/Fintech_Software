using FluentValidation;
namespace FintechApp.Requests.UserWalletMaster
{
    public class CreateUserWalletMasterRequest { public int WalletID { get; set; } public decimal Balance { get; set; } public int UserID { get; set; } public string? BalanceHash { get; set; } }
    public class CreateUserWalletMasterRequestValidator : AbstractValidator<CreateUserWalletMasterRequest> { public CreateUserWalletMasterRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.WalletID).GreaterThan(0); } }
}
