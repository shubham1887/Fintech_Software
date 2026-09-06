using FluentValidation;
namespace FintechApp.Requests.UserWalletMapping
{
    public class DeleteUserWalletMappingRequest { public int UserWalletMappingID { get; set; } }
    public class DeleteUserWalletMappingRequestValidator : AbstractValidator<DeleteUserWalletMappingRequest> { public DeleteUserWalletMappingRequestValidator() { RuleFor(x => x.UserWalletMappingID).GreaterThan(0); } }
}
