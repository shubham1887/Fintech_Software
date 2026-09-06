using FluentValidation;
namespace FintechApp.Requests.LapuPartyMaster
{
    public class DeleteLapuPartyMasterRequest { public int LapuPartyMasterID { get; set; } }
    public class DeleteLapuPartyMasterRequestValidator : AbstractValidator<DeleteLapuPartyMasterRequest> { public DeleteLapuPartyMasterRequestValidator() { RuleFor(x => x.LapuPartyMasterID).GreaterThan(0); } }
}
