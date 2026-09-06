using FluentValidation;
namespace FintechApp.Requests.eKYCMaster { public class GeteKYCMasterRequest { public int eKYCMasterID { get; set; } public int UserID { get; set; } } public class GeteKYCMasterRequestValidator : AbstractValidator<GeteKYCMasterRequest> { public GeteKYCMasterRequestValidator() { RuleFor(x => x.eKYCMasterID).GreaterThan(0); } } }
