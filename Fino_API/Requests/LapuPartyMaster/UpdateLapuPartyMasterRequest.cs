using FluentValidation;
namespace FintechApp.Requests.LapuPartyMaster
{
    public class UpdateLapuPartyMasterRequest { public int LapuPartyMasterID { get; set; } public string PartyName { get; set; } = string.Empty; public string City { get; set; } = string.Empty; public string State { get; set; } = string.Empty; public string Address { get; set; } = string.Empty; public string EmailID { get; set; } = string.Empty; public string MobileNumber { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class UpdateLapuPartyMasterRequestValidator : AbstractValidator<UpdateLapuPartyMasterRequest> { public UpdateLapuPartyMasterRequestValidator() { RuleFor(x => x.LapuPartyMasterID).GreaterThan(0); } }
}
