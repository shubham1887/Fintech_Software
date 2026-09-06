using FluentValidation;
namespace FintechApp.Requests.EcollectionDetails
{
    public class GetEcollectionDetailsRequest { public int EcollectionDetailsID { get; set; }
    public int UserID { get; set; } }
    public class GetEcollectionDetailsRequestValidator : AbstractValidator<GetEcollectionDetailsRequest> { public GetEcollectionDetailsRequestValidator() { RuleFor(x => x.EcollectionDetailsID).GreaterThan(0); } }
}
