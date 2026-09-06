using FluentValidation;
namespace FintechApp.Requests.LoanTypeMaster { public class DeleteLoanTypeMasterRequest { public int LoanTypeMasterID { get; set; } } public class DeleteLoanTypeMasterRequestValidator : AbstractValidator<DeleteLoanTypeMasterRequest> { public DeleteLoanTypeMasterRequestValidator() { RuleFor(x => x.LoanTypeMasterID).GreaterThan(0); } } }
