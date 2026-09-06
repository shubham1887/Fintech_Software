using FluentValidation;
namespace FintechApp.Requests.UpiBeneficiary { public class DeleteUpiBeneficiaryRequest { public int UpiBeneficiaryID { get; set; } } public class DeleteUpiBeneficiaryRequestValidator : AbstractValidator<DeleteUpiBeneficiaryRequest> { public DeleteUpiBeneficiaryRequestValidator() { RuleFor(x => x.UpiBeneficiaryID).GreaterThan(0); } } }
