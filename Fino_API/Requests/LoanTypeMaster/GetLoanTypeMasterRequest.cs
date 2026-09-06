using FluentValidation;
namespace FintechApp.Requests.LoanTypeMaster { public class GetLoanTypeMasterRequest { public int LoanTypeMasterID { get; set; } } public class GetLoanTypeMasterRequestValidator : AbstractValidator<GetLoanTypeMasterRequest> { public GetLoanTypeMasterRequestValidator() { RuleFor(x => x.LoanTypeMasterID).GreaterThan(0); } } }
