using FluentValidation;
namespace FintechApp.Requests.eKYCSetting { public class GeteKYCSettingRequest { public int eKYCSettingID { get; set; } } public class GeteKYCSettingRequestValidator : AbstractValidator<GeteKYCSettingRequest> { public GeteKYCSettingRequestValidator() { RuleFor(x => x.eKYCSettingID).GreaterThan(0); } } }
