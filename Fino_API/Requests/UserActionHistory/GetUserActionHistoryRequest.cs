using FluentValidation;
namespace FintechApp.Requests.UserActionHistory { public class GetUserActionHistoryRequest { public int UserHistoryID { get; set; } public int UserID { get; set; } } public class GetUserActionHistoryRequestValidator : AbstractValidator<GetUserActionHistoryRequest> { public GetUserActionHistoryRequestValidator() { RuleFor(x => x.UserHistoryID).GreaterThan(0); } } }
