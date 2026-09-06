using FluentValidation;
namespace FintechApp.Requests.eKYCSetting { public class DeleteeKYCSettingRequest { public int eKYCSettingID { get; set; } } public class DeleteeKYCSettingRequestValidator : AbstractValidator<DeleteeKYCSettingRequest> { public DeleteeKYCSettingRequestValidator() { RuleFor(x => x.eKYCSettingID).GreaterThan(0); } } }
