using FluentValidation;
namespace FintechApp.Requests.AndroidSectionMaster { public class GetAndroidSectionMasterRequest { public int AndroidSectionMasterID { get; set; } } public class GetAndroidSectionMasterRequestValidator : AbstractValidator<GetAndroidSectionMasterRequest> { public GetAndroidSectionMasterRequestValidator() { RuleFor(x => x.AndroidSectionMasterID).GreaterThan(0); } } }
