using FluentValidation;
namespace FintechApp.Requests.UserWalletMapping
{
    public class GetUserWalletMappingRequest { public int UserWalletMappingID { get; set; } public int UserID { get; set; } }
    public class GetUserWalletMappingRequestValidator : AbstractValidator<GetUserWalletMappingRequest> { public GetUserWalletMappingRequestValidator() { RuleFor(x => x.UserWalletMappingID).GreaterThan(0); } }
}
