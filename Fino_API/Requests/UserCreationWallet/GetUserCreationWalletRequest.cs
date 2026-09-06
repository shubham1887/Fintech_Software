using FluentValidation;
namespace FintechApp.Requests.UserCreationWallet
{
    public class GetUserCreationWalletRequest { public int UserCreationWalletID { get; set; } public int UserID { get; set; } }
    public class GetUserCreationWalletRequestValidator : AbstractValidator<GetUserCreationWalletRequest> { public GetUserCreationWalletRequestValidator() { RuleFor(x => x.UserCreationWalletID).GreaterThan(0); } }
}
