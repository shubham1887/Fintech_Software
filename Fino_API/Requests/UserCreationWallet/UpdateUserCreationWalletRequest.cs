using FluentValidation;
namespace FintechApp.Requests.UserCreationWallet
{
    public class UpdateUserCreationWalletRequest { public int UserCreationWalletID { get; set; } public int IdCount { get; set; } public int UserID { get; set; } }
    public class UpdateUserCreationWalletRequestValidator : AbstractValidator<UpdateUserCreationWalletRequest> { public UpdateUserCreationWalletRequestValidator() { RuleFor(x => x.UserCreationWalletID).GreaterThan(0); } }
}
