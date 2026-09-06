using FluentValidation;
namespace FintechApp.Requests.CircleAPIMaster
{
    public class CreateCircleAPIMasterRequest { public string CircleAPIURL { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class CreateCircleAPIMasterRequestValidator : AbstractValidator<CreateCircleAPIMasterRequest> { public CreateCircleAPIMasterRequestValidator() {  } }
}
