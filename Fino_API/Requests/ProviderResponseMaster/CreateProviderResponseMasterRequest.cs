using FluentValidation;
namespace FintechApp.Requests.ProviderResponseMaster
{
    public class CreateProviderResponseMasterRequest { public int ProviderID { get; set; } public string TextMustexits { get; set; } = string.Empty; public string Type { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; public bool IsReference { get; set; } public bool IsActive { get; set; } }
    public class CreateProviderResponseMasterRequestValidator : AbstractValidator<CreateProviderResponseMasterRequest> { public CreateProviderResponseMasterRequestValidator() { RuleFor(x => x.ProviderID).GreaterThan(0); RuleFor(x => x.TextMustexits).NotEmpty(); } }
}
