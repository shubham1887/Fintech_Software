using FluentValidation;
namespace FintechApp.Requests.PaySprintBank { public class DeletePaySprintBankRequest { public int ID { get; set; } } public class DeletePaySprintBankRequestValidator : AbstractValidator<DeletePaySprintBankRequest> { public DeletePaySprintBankRequestValidator() { RuleFor(x => x.ID).GreaterThan(0); } } }
