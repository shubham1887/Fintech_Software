using FluentValidation;
namespace FintechApp.Requests.ProviderUserMapping
{
    public class DeleteProviderUserMappingRequest { public int ProviderUserMappingID { get; set; } }
    public class DeleteProviderUserMappingRequestValidator : AbstractValidator<DeleteProviderUserMappingRequest> { public DeleteProviderUserMappingRequestValidator() { RuleFor(x => x.ProviderUserMappingID).GreaterThan(0); } }
}
