using FluentValidation;
namespace FintechApp.Requests.FingerPrintDeviceType
{
    public class UpdateFingerPrintDeviceTypeRequest { public int FingerPrintDeviceTypeID { get; set; } public string DeviceName { get; set; } = string.Empty; public string Value { get; set; } = string.Empty; }
    public class UpdateFingerPrintDeviceTypeRequestValidator : AbstractValidator<UpdateFingerPrintDeviceTypeRequest> { public UpdateFingerPrintDeviceTypeRequestValidator() { RuleFor(x => x.FingerPrintDeviceTypeID).GreaterThan(0); } }
}
