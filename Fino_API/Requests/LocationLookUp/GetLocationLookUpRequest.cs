using FluentValidation;
namespace FintechApp.Requests.LocationLookUp
{
    public class GetLocationLookUpRequest { public int LocationLookUpID { get; set; } }
    public class GetLocationLookUpRequestValidator : AbstractValidator<GetLocationLookUpRequest> { public GetLocationLookUpRequestValidator() { RuleFor(x => x.LocationLookUpID).GreaterThan(0); } }
}
