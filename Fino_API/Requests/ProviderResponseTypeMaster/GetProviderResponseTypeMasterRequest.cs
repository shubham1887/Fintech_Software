using FluentValidation;
namespace FintechApp.Requests.ProviderResponseTypeMaster
{
    public class GetProviderResponseTypeMasterRequest { public int ProviderResponseTypeMasterID { get; set; } }
    public class GetProviderResponseTypeMasterRequestValidator : AbstractValidator<GetProviderResponseTypeMasterRequest> { public GetProviderResponseTypeMasterRequestValidator() { RuleFor(x => x.ProviderResponseTypeMasterID).GreaterThan(0); } }
}
