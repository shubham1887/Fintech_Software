using FluentValidation;
namespace FintechApp.Requests.MPinHistory { public class GetMPinHistoryRequest { public int MPinHistoryID { get; set; } public int UserID { get; set; } } public class GetMPinHistoryRequestValidator : AbstractValidator<GetMPinHistoryRequest> { public GetMPinHistoryRequestValidator() { RuleFor(x => x.MPinHistoryID).GreaterThan(0); } } }
