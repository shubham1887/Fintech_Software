using FluentValidation;
namespace FintechApp.Requests.PrepaidActivation { public class DeletePrepaidActivationRequest { public int PrepaidActivationID { get; set; } } public class DeletePrepaidActivationRequestValidator : AbstractValidator<DeletePrepaidActivationRequest> { public DeletePrepaidActivationRequestValidator() { RuleFor(x => x.PrepaidActivationID).GreaterThan(0); } } }
