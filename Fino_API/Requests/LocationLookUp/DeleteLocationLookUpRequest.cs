using FluentValidation;
namespace FintechApp.Requests.LocationLookUp
{
    public class DeleteLocationLookUpRequest { public int LocationLookUpID { get; set; } }
    public class DeleteLocationLookUpRequestValidator : AbstractValidator<DeleteLocationLookUpRequest> { public DeleteLocationLookUpRequestValidator() { RuleFor(x => x.LocationLookUpID).GreaterThan(0); } }
}
