using FluentValidation;
namespace FintechApp.Requests.WalletServiceMapping { public class CreateWalletServiceMappingRequest { public int WalletID { get; set; } public int ServiceID { get; set; } } public class CreateWalletServiceMappingRequestValidator : AbstractValidator<CreateWalletServiceMappingRequest> { public CreateWalletServiceMappingRequestValidator() {  } } }
