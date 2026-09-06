using FluentValidation;
namespace FintechApp.Requests.LapuMaster
{
    public class CreateLapuMasterRequest { public int LapuPartyMasterID { get; set; } public int OperatorID { get; set; } public string LapuNo { get; set; } = string.Empty; public string ProivderLapuID { get; set; } = string.Empty; public decimal ClosingBalance { get; set; } }
    public class CreateLapuMasterRequestValidator : AbstractValidator<CreateLapuMasterRequest> { public CreateLapuMasterRequestValidator() {  } }
}
