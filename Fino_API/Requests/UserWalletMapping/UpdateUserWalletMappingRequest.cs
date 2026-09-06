using FluentValidation;
namespace FintechApp.Requests.UserWalletMapping
{
    public class UpdateUserWalletMappingRequest { public int UserWalletMappingID { get; set; } public int UserID { get; set; } public int WalletID { get; set; } }
    public class UpdateUserWalletMappingRequestValidator : AbstractValidator<UpdateUserWalletMappingRequest> { public UpdateUserWalletMappingRequestValidator() { RuleFor(x => x.UserWalletMappingID).GreaterThan(0); } }
}
