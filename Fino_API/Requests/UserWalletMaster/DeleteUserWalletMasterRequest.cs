using FluentValidation;
namespace FintechApp.Requests.UserWalletMaster
{
    public class DeleteUserWalletMasterRequest { public int UserWalletMasterID { get; set; } }
    public class DeleteUserWalletMasterRequestValidator : AbstractValidator<DeleteUserWalletMasterRequest> { public DeleteUserWalletMasterRequestValidator() { RuleFor(x => x.UserWalletMasterID).GreaterThan(0); } }
}
