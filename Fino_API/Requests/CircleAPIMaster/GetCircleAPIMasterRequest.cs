using FluentValidation;
namespace FintechApp.Requests.CircleAPIMaster
{
    public class GetCircleAPIMasterRequest { public int CircleAPIMasterID { get; set; } }
    public class GetCircleAPIMasterRequestValidator : AbstractValidator<GetCircleAPIMasterRequest> { public GetCircleAPIMasterRequestValidator() { RuleFor(x => x.CircleAPIMasterID).GreaterThan(0); } }
}
