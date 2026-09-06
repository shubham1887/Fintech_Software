using FluentValidation;
namespace FintechApp.Requests.UserwiseDMTMapping
{
    public class UpdateUserwiseDMTMappingRequest { public int UserwiseDMTMappingID { get; set; } public int UserID { get; set; } public int ServiceID { get; set; } public int OperatorID { get; set; } public int ProviderID { get; set; } }
    public class UpdateUserwiseDMTMappingRequestValidator : AbstractValidator<UpdateUserwiseDMTMappingRequest> { public UpdateUserwiseDMTMappingRequestValidator() { RuleFor(x => x.UserwiseDMTMappingID).GreaterThan(0); } }
}
