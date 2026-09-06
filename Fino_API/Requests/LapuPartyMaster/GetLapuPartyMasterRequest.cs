using FluentValidation;
namespace FintechApp.Requests.LapuPartyMaster
{
    public class GetLapuPartyMasterRequest { public int LapuPartyMasterID { get; set; } }
    public class GetLapuPartyMasterRequestValidator : AbstractValidator<GetLapuPartyMasterRequest> { public GetLapuPartyMasterRequestValidator() { RuleFor(x => x.LapuPartyMasterID).GreaterThan(0); } }
}
