using FluentValidation;
namespace FintechApp.Requests.CircleAPIMaster
{
    public class UpdateCircleAPIMasterRequest { public int CircleAPIMasterID { get; set; } public string CircleAPIURL { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class UpdateCircleAPIMasterRequestValidator : AbstractValidator<UpdateCircleAPIMasterRequest> { public UpdateCircleAPIMasterRequestValidator() { RuleFor(x => x.CircleAPIMasterID).GreaterThan(0); } }
}
