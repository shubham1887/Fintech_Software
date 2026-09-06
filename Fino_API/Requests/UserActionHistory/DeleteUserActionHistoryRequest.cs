using FluentValidation;
namespace FintechApp.Requests.UserActionHistory { public class DeleteUserActionHistoryRequest { public int UserHistoryID { get; set; } } public class DeleteUserActionHistoryRequestValidator : AbstractValidator<DeleteUserActionHistoryRequest> { public DeleteUserActionHistoryRequestValidator() { RuleFor(x => x.UserHistoryID).GreaterThan(0); } } }
