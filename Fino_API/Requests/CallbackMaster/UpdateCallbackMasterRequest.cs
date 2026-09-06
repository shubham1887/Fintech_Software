using FluentValidation;
namespace FintechApp.Requests.CallbackMaster
{
    public class UpdateCallbackMasterRequest { public int CallbackMasterID { get; set; } public int UserID { get; set; } public string? CallbackURL { get; set; } public bool IsDelete { get; set; } public int ServiceID { get; set; } }
    public class UpdateCallbackMasterRequestValidator : AbstractValidator<UpdateCallbackMasterRequest> { public UpdateCallbackMasterRequestValidator() { RuleFor(x => x.CallbackMasterID).GreaterThan(0); } }
}
