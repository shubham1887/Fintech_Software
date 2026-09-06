using FluentValidation;
namespace FintechApp.Requests.LapuMaster
{
    public class UpdateLapuMasterRequest { public int LapuMasterID { get; set; } public int LapuPartyMasterID { get; set; } public int OperatorID { get; set; } public string LapuNo { get; set; } = string.Empty; public string ProivderLapuID { get; set; } = string.Empty; public decimal ClosingBalance { get; set; } }
    public class UpdateLapuMasterRequestValidator : AbstractValidator<UpdateLapuMasterRequest> { public UpdateLapuMasterRequestValidator() { RuleFor(x => x.LapuMasterID).GreaterThan(0); } }
}
