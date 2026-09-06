using FluentValidation;
namespace FintechApp.Requests.CircleAPIMaster
{
    public class DeleteCircleAPIMasterRequest { public int CircleAPIMasterID { get; set; } }
    public class DeleteCircleAPIMasterRequestValidator : AbstractValidator<DeleteCircleAPIMasterRequest> { public DeleteCircleAPIMasterRequestValidator() { RuleFor(x => x.CircleAPIMasterID).GreaterThan(0); } }
}
