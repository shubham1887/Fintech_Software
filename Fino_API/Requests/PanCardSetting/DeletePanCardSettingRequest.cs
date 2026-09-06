using FluentValidation;
namespace FintechApp.Requests.PanCardSetting { public class DeletePanCardSettingRequest { public int PanCardSettingID { get; set; } } public class DeletePanCardSettingRequestValidator : AbstractValidator<DeletePanCardSettingRequest> { public DeletePanCardSettingRequestValidator() { RuleFor(x => x.PanCardSettingID).GreaterThan(0); } } }
