using FluentValidation;
namespace FintechApp.Requests.UserwiseDMTMapping
{
    public class DeleteUserwiseDMTMappingRequest { public int UserwiseDMTMappingID { get; set; } }
    public class DeleteUserwiseDMTMappingRequestValidator : AbstractValidator<DeleteUserwiseDMTMappingRequest> { public DeleteUserwiseDMTMappingRequestValidator() { RuleFor(x => x.UserwiseDMTMappingID).GreaterThan(0); } }
}
