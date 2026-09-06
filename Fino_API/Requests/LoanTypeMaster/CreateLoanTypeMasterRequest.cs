using FluentValidation;
namespace FintechApp.Requests.LoanTypeMaster { public class CreateLoanTypeMasterRequest { public string LoanType { get; set; } = string.Empty; } public class CreateLoanTypeMasterRequestValidator : AbstractValidator<CreateLoanTypeMasterRequest> { public CreateLoanTypeMasterRequestValidator() {  } } }
