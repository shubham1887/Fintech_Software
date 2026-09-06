using FluentValidation;
namespace FintechApp.Requests.UpiBeneficiary { public class GetUpiBeneficiaryRequest { public int UpiBeneficiaryID { get; set; } public int UserID { get; set; } } public class GetUpiBeneficiaryRequestValidator : AbstractValidator<GetUpiBeneficiaryRequest> { public GetUpiBeneficiaryRequestValidator() { RuleFor(x => x.UpiBeneficiaryID).GreaterThan(0); } } }
