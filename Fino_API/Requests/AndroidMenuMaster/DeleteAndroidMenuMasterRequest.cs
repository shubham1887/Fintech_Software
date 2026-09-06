using FluentValidation;
namespace FintechApp.Requests.AndroidMenuMaster { public class DeleteAndroidMenuMasterRequest { public int AndroidMenuMasterID { get; set; } } public class DeleteAndroidMenuMasterRequestValidator : AbstractValidator<DeleteAndroidMenuMasterRequest> { public DeleteAndroidMenuMasterRequestValidator() { RuleFor(x => x.AndroidMenuMasterID).GreaterThan(0); } } }
