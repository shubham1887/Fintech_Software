using FluentValidation;
namespace FintechApp.Requests.PaySprintBank { public class GetPaySprintBankRequest { public int ID { get; set; } } public class GetPaySprintBankRequestValidator : AbstractValidator<GetPaySprintBankRequest> { public GetPaySprintBankRequestValidator() { RuleFor(x => x.ID).GreaterThan(0); } } }
