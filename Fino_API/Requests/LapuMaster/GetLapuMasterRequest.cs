using FluentValidation;
namespace FintechApp.Requests.LapuMaster
{
    public class GetLapuMasterRequest { public int LapuMasterID { get; set; } }
    public class GetLapuMasterRequestValidator : AbstractValidator<GetLapuMasterRequest> { public GetLapuMasterRequestValidator() { RuleFor(x => x.LapuMasterID).GreaterThan(0); } }
}
