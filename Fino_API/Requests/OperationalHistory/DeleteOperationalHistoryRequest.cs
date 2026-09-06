using FluentValidation;
namespace FintechApp.Requests.OperationalHistory { public class DeleteOperationalHistoryRequest { public int OperationalHistoryID { get; set; } } public class DeleteOperationalHistoryRequestValidator : AbstractValidator<DeleteOperationalHistoryRequest> { public DeleteOperationalHistoryRequestValidator() { RuleFor(x => x.OperationalHistoryID).GreaterThan(0); } } }
