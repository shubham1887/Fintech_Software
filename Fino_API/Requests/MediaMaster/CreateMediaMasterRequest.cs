using FluentValidation;
namespace FintechApp.Requests.MediaMaster
{
    public class CreateMediaMasterRequest { public int UserID { get; set; } public string MediaType { get; set; } = string.Empty; public string MediaPath { get; set; } = string.Empty; }
    public class CreateMediaMasterRequestValidator : AbstractValidator<CreateMediaMasterRequest> { public CreateMediaMasterRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
