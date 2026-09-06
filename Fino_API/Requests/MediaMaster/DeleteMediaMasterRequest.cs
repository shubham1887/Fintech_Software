using FluentValidation;
namespace FintechApp.Requests.MediaMaster
{
    public class DeleteMediaMasterRequest { public int MediaMasterID { get; set; } }
    public class DeleteMediaMasterRequestValidator : AbstractValidator<DeleteMediaMasterRequest> { public DeleteMediaMasterRequestValidator() { RuleFor(x => x.MediaMasterID).GreaterThan(0); } }
}
