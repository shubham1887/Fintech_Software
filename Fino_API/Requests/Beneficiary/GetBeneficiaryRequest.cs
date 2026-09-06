using FluentValidation;
namespace FintechApp.Requests.Beneficiary { public class GetBeneficiaryRequest { public int BeneficiaryID { get; set; } public int UserID { get; set; } } public class GetBeneficiaryRequestValidator : AbstractValidator<GetBeneficiaryRequest> { public GetBeneficiaryRequestValidator() { RuleFor(x => x.BeneficiaryID).GreaterThan(0); } } }
