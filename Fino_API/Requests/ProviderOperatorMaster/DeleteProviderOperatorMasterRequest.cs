using FluentValidation;
namespace FintechApp.Requests.ProviderOperatorMaster
{
    public class DeleteProviderOperatorMasterRequest { public int ProviderOperatorID { get; set; } }
    public class DeleteProviderOperatorMasterRequestValidator : AbstractValidator<DeleteProviderOperatorMasterRequest> { public DeleteProviderOperatorMasterRequestValidator() { RuleFor(x => x.ProviderOperatorID).GreaterThan(0); } }
}
