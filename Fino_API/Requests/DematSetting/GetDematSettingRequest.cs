using FluentValidation;
namespace FintechApp.Requests.DematSetting { public class GetDematSettingRequest { public int DematSettingID { get; set; } } public class GetDematSettingRequestValidator : AbstractValidator<GetDematSettingRequest> { public GetDematSettingRequestValidator() { RuleFor(x => x.DematSettingID).GreaterThan(0); } } }
