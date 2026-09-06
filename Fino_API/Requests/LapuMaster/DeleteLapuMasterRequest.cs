using FluentValidation;
namespace FintechApp.Requests.LapuMaster
{
    public class DeleteLapuMasterRequest { public int LapuMasterID { get; set; } }
    public class DeleteLapuMasterRequestValidator : AbstractValidator<DeleteLapuMasterRequest> { public DeleteLapuMasterRequestValidator() { RuleFor(x => x.LapuMasterID).GreaterThan(0); } }
}
