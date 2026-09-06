using FluentValidation;
namespace FintechApp.Requests.UserBalanceCapping
{
    public class GetUserBalanceCappingRequest { public int UserBalanceCappingID { get; set; } public int UserID { get; set; } }
    public class GetUserBalanceCappingRequestValidator : AbstractValidator<GetUserBalanceCappingRequest> { public GetUserBalanceCappingRequestValidator() { RuleFor(x => x.UserBalanceCappingID).GreaterThan(0); } }
}
