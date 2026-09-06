using FluentValidation;
namespace FintechApp.Requests.ProviderResponseMaster
{
    public class DeleteProviderResponseMasterRequest { public int ProviderResponseMasterID { get; set; } }
    public class DeleteProviderResponseMasterRequestValidator : AbstractValidator<DeleteProviderResponseMasterRequest> { public DeleteProviderResponseMasterRequestValidator() { RuleFor(x => x.ProviderResponseMasterID).GreaterThan(0); } }
}
