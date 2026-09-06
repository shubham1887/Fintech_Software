using FluentValidation;
namespace FintechApp.Requests.ProviderResponseTypeMaster
{
    public class DeleteProviderResponseTypeMasterRequest { public int ProviderResponseTypeMasterID { get; set; } }
    public class DeleteProviderResponseTypeMasterRequestValidator : AbstractValidator<DeleteProviderResponseTypeMasterRequest> { public DeleteProviderResponseTypeMasterRequestValidator() { RuleFor(x => x.ProviderResponseTypeMasterID).GreaterThan(0); } }
}
