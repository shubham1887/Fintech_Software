using FluentValidation;
namespace FintechApp.Requests.UserWalletMaster
{
    public class GetUserWalletMasterRequest { public int UserWalletMasterID { get; set; } public int UserID { get; set; } }
    public class GetUserWalletMasterRequestValidator : AbstractValidator<GetUserWalletMasterRequest> { public GetUserWalletMasterRequestValidator() { RuleFor(x => x.UserWalletMasterID).GreaterThan(0); } }
}
