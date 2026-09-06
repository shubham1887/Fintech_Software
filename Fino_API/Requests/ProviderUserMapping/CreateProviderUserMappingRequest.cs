using FluentValidation;
namespace FintechApp.Requests.ProviderUserMapping
{
    public class CreateProviderUserMappingRequest { public string Key { get; set; } = string.Empty; public string Value { get; set; } = string.Empty; public int ProviderID { get; set; } public int UserID { get; set; } }
    public class CreateProviderUserMappingRequestValidator : AbstractValidator<CreateProviderUserMappingRequest> { public CreateProviderUserMappingRequestValidator() { RuleFor(x => x.Key).NotEmpty(); RuleFor(x => x.UserID).GreaterThan(0); } }
}
