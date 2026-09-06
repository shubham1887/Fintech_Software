using FluentValidation;
namespace FintechApp.Requests.MPinHistory { public class DeleteMPinHistoryRequest { public int MPinHistoryID { get; set; } } public class DeleteMPinHistoryRequestValidator : AbstractValidator<DeleteMPinHistoryRequest> { public DeleteMPinHistoryRequestValidator() { RuleFor(x => x.MPinHistoryID).GreaterThan(0); } } }
