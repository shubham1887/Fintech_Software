using FluentValidation;
namespace FintechApp.Requests.UserwiseDMTMapping
{
    public class GetUserwiseDMTMappingRequest { public int UserwiseDMTMappingID { get; set; } public int UserID { get; set; } }
    public class GetUserwiseDMTMappingRequestValidator : AbstractValidator<GetUserwiseDMTMappingRequest> { public GetUserwiseDMTMappingRequestValidator() { RuleFor(x => x.UserwiseDMTMappingID).GreaterThan(0); } }
}
