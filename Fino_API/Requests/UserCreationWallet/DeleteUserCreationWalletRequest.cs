using FluentValidation;
namespace FintechApp.Requests.UserCreationWallet
{
    public class DeleteUserCreationWalletRequest { public int UserCreationWalletID { get; set; } }
    public class DeleteUserCreationWalletRequestValidator : AbstractValidator<DeleteUserCreationWalletRequest> { public DeleteUserCreationWalletRequestValidator() { RuleFor(x => x.UserCreationWalletID).GreaterThan(0); } }
}
