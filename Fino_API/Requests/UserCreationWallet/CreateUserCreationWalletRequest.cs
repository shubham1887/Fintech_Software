using FluentValidation;
namespace FintechApp.Requests.UserCreationWallet
{
    public class CreateUserCreationWalletRequest { public int IdCount { get; set; } public int UserID { get; set; } }
    public class CreateUserCreationWalletRequestValidator : AbstractValidator<CreateUserCreationWalletRequest> { public CreateUserCreationWalletRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
