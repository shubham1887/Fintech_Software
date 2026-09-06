using FluentValidation;
namespace FintechApp.Requests.UserWalletMapping
{
    public class CreateUserWalletMappingRequest { public int UserID { get; set; } public int WalletID { get; set; } }
    public class CreateUserWalletMappingRequestValidator : AbstractValidator<CreateUserWalletMappingRequest> { public CreateUserWalletMappingRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.WalletID).GreaterThan(0); } }
}
