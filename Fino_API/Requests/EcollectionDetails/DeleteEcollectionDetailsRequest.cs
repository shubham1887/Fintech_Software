using FluentValidation;
namespace FintechApp.Requests.EcollectionDetails
{
    public class DeleteEcollectionDetailsRequest { public int EcollectionDetailsID { get; set; } }
    public class DeleteEcollectionDetailsRequestValidator : AbstractValidator<DeleteEcollectionDetailsRequest> { public DeleteEcollectionDetailsRequestValidator() { RuleFor(x => x.EcollectionDetailsID).GreaterThan(0); } }
}
