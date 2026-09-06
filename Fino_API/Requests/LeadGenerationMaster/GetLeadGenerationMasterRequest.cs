using FluentValidation;
namespace FintechApp.Requests.LeadGenerationMaster { public class GetLeadGenerationMasterRequest { public int LeadGenerationMasterID { get; set; } } public class GetLeadGenerationMasterRequestValidator : AbstractValidator<GetLeadGenerationMasterRequest> { public GetLeadGenerationMasterRequestValidator() { RuleFor(x => x.LeadGenerationMasterID).GreaterThan(0); } } }
