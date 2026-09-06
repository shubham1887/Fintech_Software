using FluentValidation;
namespace FintechApp.Requests.WalletServiceMapping { public class GetWalletServiceMappingRequest { public int WalletServiceMappingID { get; set; } } public class GetWalletServiceMappingRequestValidator : AbstractValidator<GetWalletServiceMappingRequest> { public GetWalletServiceMappingRequestValidator() { RuleFor(x => x.WalletServiceMappingID).GreaterThan(0); } } }
