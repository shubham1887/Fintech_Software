using FluentValidation;
namespace FintechApp.Requests.MediaMaster
{
    public class UpdateMediaMasterRequest { public int MediaMasterID { get; set; } public int UserID { get; set; } public string MediaType { get; set; } = string.Empty; public string MediaPath { get; set; } = string.Empty; }
    public class UpdateMediaMasterRequestValidator : AbstractValidator<UpdateMediaMasterRequest> { public UpdateMediaMasterRequestValidator() { RuleFor(x => x.MediaMasterID).GreaterThan(0); } }
}
