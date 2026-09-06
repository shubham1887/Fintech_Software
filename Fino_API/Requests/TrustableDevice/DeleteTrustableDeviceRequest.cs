using FluentValidation;
namespace FintechApp.Requests.TrustableDevice { public class DeleteTrustableDeviceRequest { public int ID { get; set; } } public class DeleteTrustableDeviceRequestValidator : AbstractValidator<DeleteTrustableDeviceRequest> { public DeleteTrustableDeviceRequestValidator() { RuleFor(x => x.ID).GreaterThan(0); } } }
