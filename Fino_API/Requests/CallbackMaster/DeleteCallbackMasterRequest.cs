using FluentValidation;
namespace FintechApp.Requests.CallbackMaster
{
    public class DeleteCallbackMasterRequest { public int CallbackMasterID { get; set; } }
    public class DeleteCallbackMasterRequestValidator : AbstractValidator<DeleteCallbackMasterRequest> { public DeleteCallbackMasterRequestValidator() { RuleFor(x => x.CallbackMasterID).GreaterThan(0); } }
}
