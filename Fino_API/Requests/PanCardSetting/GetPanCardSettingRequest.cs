using FluentValidation;
namespace FintechApp.Requests.PanCardSetting { public class GetPanCardSettingRequest { public int PanCardSettingID { get; set; } } public class GetPanCardSettingRequestValidator : AbstractValidator<GetPanCardSettingRequest> { public GetPanCardSettingRequestValidator() { RuleFor(x => x.PanCardSettingID).GreaterThan(0); } } }
