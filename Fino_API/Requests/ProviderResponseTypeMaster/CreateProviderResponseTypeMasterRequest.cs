using FluentValidation;
namespace FintechApp.Requests.ProviderResponseTypeMaster
{
    public class CreateProviderResponseTypeMasterRequest { public string Type { get; set; } = string.Empty; }
    public class CreateProviderResponseTypeMasterRequestValidator : AbstractValidator<CreateProviderResponseTypeMasterRequest> { public CreateProviderResponseTypeMasterRequestValidator() {  } }
}
