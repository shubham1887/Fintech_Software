using FluentValidation;
namespace FintechApp.Requests.TrustableDevice { public class GetTrustableDeviceRequest { public int ID { get; set; } public int UserID { get; set; } } public class GetTrustableDeviceRequestValidator : AbstractValidator<GetTrustableDeviceRequest> { public GetTrustableDeviceRequestValidator() { RuleFor(x => x.ID).GreaterThan(0); } } }
