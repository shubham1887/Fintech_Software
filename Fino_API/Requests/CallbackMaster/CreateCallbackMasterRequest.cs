using FluentValidation;
namespace FintechApp.Requests.CallbackMaster
{
    public class CreateCallbackMasterRequest { public int UserID { get; set; } public string? CallbackURL { get; set; } public bool IsDelete { get; set; } public int ServiceID { get; set; } }
    public class CreateCallbackMasterRequestValidator : AbstractValidator<CreateCallbackMasterRequest> { public CreateCallbackMasterRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.ServiceID).GreaterThan(0); } }
}
