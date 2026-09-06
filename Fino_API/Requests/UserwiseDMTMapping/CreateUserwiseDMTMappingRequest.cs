using FluentValidation;
namespace FintechApp.Requests.UserwiseDMTMapping
{
    public class CreateUserwiseDMTMappingRequest { public int UserID { get; set; } public int ServiceID { get; set; } public int OperatorID { get; set; } public int ProviderID { get; set; } }
    public class CreateUserwiseDMTMappingRequestValidator : AbstractValidator<CreateUserwiseDMTMappingRequest> { public CreateUserwiseDMTMappingRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.ServiceID).GreaterThan(0); RuleFor(x => x.OperatorID).GreaterThan(0); RuleFor(x => x.ProviderID).GreaterThan(0); } }
}
