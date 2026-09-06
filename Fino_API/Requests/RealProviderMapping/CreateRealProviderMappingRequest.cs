using FluentValidation;
namespace FintechApp.Requests.RealProviderMapping
{
    public class CreateRealProviderMappingRequest { public int ProviderID { get; set; } public int OperatorID { get; set; } }
    public class CreateRealProviderMappingRequestValidator : AbstractValidator<CreateRealProviderMappingRequest> { public CreateRealProviderMappingRequestValidator() {  } }
}
