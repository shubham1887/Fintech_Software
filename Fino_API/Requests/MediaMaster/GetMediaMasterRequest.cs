using FluentValidation;
namespace FintechApp.Requests.MediaMaster
{
    public class GetMediaMasterRequest { public int MediaMasterID { get; set; }
    public int UserID { get; set; } }
    public class GetMediaMasterRequestValidator : AbstractValidator<GetMediaMasterRequest> { public GetMediaMasterRequestValidator() { RuleFor(x => x.MediaMasterID).GreaterThan(0); } }
}
