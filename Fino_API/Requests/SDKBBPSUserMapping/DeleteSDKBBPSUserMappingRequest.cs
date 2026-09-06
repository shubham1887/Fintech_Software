using FluentValidation;
namespace FintechApp.Requests.SDKBBPSUserMapping { public class DeleteSDKBBPSUserMappingRequest { public int SDKBBPSUserMappingID { get; set; } } public class DeleteSDKBBPSUserMappingRequestValidator : AbstractValidator<DeleteSDKBBPSUserMappingRequest> { public DeleteSDKBBPSUserMappingRequestValidator() { RuleFor(x => x.SDKBBPSUserMappingID).GreaterThan(0); } } }
