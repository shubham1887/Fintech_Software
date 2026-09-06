using FluentValidation;
namespace FintechApp.Requests.eKYCMaster { public class DeleteeKYCMasterRequest { public int eKYCMasterID { get; set; } } public class DeleteeKYCMasterRequestValidator : AbstractValidator<DeleteeKYCMasterRequest> { public DeleteeKYCMasterRequestValidator() { RuleFor(x => x.eKYCMasterID).GreaterThan(0); } } }
