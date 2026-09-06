using FluentValidation;
namespace FintechApp.Requests.PaySprintBank { public class CreatePaySprintBankRequest { public string BankName { get; set; } = string.Empty; public int Sequence { get; set; } } public class CreatePaySprintBankRequestValidator : AbstractValidator<CreatePaySprintBankRequest> { public CreatePaySprintBankRequestValidator() {  } } }
