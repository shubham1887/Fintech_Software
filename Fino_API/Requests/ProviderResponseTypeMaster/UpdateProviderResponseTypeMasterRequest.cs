using FluentValidation;
namespace FintechApp.Requests.ProviderResponseTypeMaster
{
    public class UpdateProviderResponseTypeMasterRequest { public int ProviderResponseTypeMasterID { get; set; } public string Type { get; set; } = string.Empty; }
    public class UpdateProviderResponseTypeMasterRequestValidator : AbstractValidator<UpdateProviderResponseTypeMasterRequest> { public UpdateProviderResponseTypeMasterRequestValidator() { RuleFor(x => x.ProviderResponseTypeMasterID).GreaterThan(0); } }
}
