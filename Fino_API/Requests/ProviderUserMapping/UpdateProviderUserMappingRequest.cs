using FluentValidation;
namespace FintechApp.Requests.ProviderUserMapping
{
    public class UpdateProviderUserMappingRequest { public int ProviderUserMappingID { get; set; } public string Key { get; set; } = string.Empty; public string Value { get; set; } = string.Empty; public int ProviderID { get; set; } public int UserID { get; set; } }
    public class UpdateProviderUserMappingRequestValidator : AbstractValidator<UpdateProviderUserMappingRequest> { public UpdateProviderUserMappingRequestValidator() { RuleFor(x => x.ProviderUserMappingID).GreaterThan(0); } }
}
