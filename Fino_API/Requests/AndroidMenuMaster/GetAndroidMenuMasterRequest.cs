using FluentValidation;
namespace FintechApp.Requests.AndroidMenuMaster { public class GetAndroidMenuMasterRequest { public int AndroidMenuMasterID { get; set; } } public class GetAndroidMenuMasterRequestValidator : AbstractValidator<GetAndroidMenuMasterRequest> { public GetAndroidMenuMasterRequestValidator() { RuleFor(x => x.AndroidMenuMasterID).GreaterThan(0); } } }
