using FluentValidation;
namespace FintechApp.Requests.UserBalanceCapping
{
    public class DeleteUserBalanceCappingRequest { public int UserBalanceCappingID { get; set; } }
    public class DeleteUserBalanceCappingRequestValidator : AbstractValidator<DeleteUserBalanceCappingRequest> { public DeleteUserBalanceCappingRequestValidator() { RuleFor(x => x.UserBalanceCappingID).GreaterThan(0); } }
}
