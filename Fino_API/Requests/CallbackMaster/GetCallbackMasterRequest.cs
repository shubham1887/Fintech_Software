using FluentValidation;
namespace FintechApp.Requests.CallbackMaster
{
    public class GetCallbackMasterRequest { public int CallbackMasterID { get; set; } public int UserID { get; set; } }
    public class GetCallbackMasterRequestValidator : AbstractValidator<GetCallbackMasterRequest> { public GetCallbackMasterRequestValidator() { RuleFor(x => x.CallbackMasterID).GreaterThan(0); } }
}
