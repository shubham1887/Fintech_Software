using FluentValidation;
namespace FintechApp.Requests.FingerPrintDeviceType
{
    public class CreateFingerPrintDeviceTypeRequest { public string DeviceName { get; set; } = string.Empty; public string Value { get; set; } = string.Empty; }
    public class CreateFingerPrintDeviceTypeRequestValidator : AbstractValidator<CreateFingerPrintDeviceTypeRequest> { public CreateFingerPrintDeviceTypeRequestValidator() {  } }
}
