using FluentValidation;
namespace FintechApp.Requests.ProviderResponseMaster
{
    public class GetProviderResponseMasterRequest { public int ProviderResponseMasterID { get; set; } public int ProviderID { get; set; } }
    public class GetProviderResponseMasterRequestValidator : AbstractValidator<GetProviderResponseMasterRequest> { public GetProviderResponseMasterRequestValidator() { RuleFor(x => x.ProviderResponseMasterID).GreaterThan(0); } }
}
