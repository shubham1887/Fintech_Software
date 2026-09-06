using FluentValidation;
namespace FintechApp.Requests.Beneficiary { public class DeleteBeneficiaryRequest { public int BeneficiaryID { get; set; } } public class DeleteBeneficiaryRequestValidator : AbstractValidator<DeleteBeneficiaryRequest> { public DeleteBeneficiaryRequestValidator() { RuleFor(x => x.BeneficiaryID).GreaterThan(0); } } }
