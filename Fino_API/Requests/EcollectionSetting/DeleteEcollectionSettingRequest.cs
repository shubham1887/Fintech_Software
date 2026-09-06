using FluentValidation;
namespace FintechApp.Requests.EcollectionSetting
{
    public class DeleteEcollectionSettingRequest { public int EcollectionSettingID { get; set; } }
    public class DeleteEcollectionSettingRequestValidator : AbstractValidator<DeleteEcollectionSettingRequest> { public DeleteEcollectionSettingRequestValidator() { RuleFor(x => x.EcollectionSettingID).GreaterThan(0); } }
}
