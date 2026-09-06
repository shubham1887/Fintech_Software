using FluentValidation;
namespace FintechApp.Requests.FingerPrintDeviceType
{
    public class GetFingerPrintDeviceTypeRequest { public int FingerPrintDeviceTypeID { get; set; } }
    public class GetFingerPrintDeviceTypeRequestValidator : AbstractValidator<GetFingerPrintDeviceTypeRequest> { public GetFingerPrintDeviceTypeRequestValidator() { RuleFor(x => x.FingerPrintDeviceTypeID).GreaterThan(0); } }
}
