using FluentValidation;
namespace FintechApp.Requests.FingerPrintDeviceType
{
    public class DeleteFingerPrintDeviceTypeRequest { public int FingerPrintDeviceTypeID { get; set; } }
    public class DeleteFingerPrintDeviceTypeRequestValidator : AbstractValidator<DeleteFingerPrintDeviceTypeRequest> { public DeleteFingerPrintDeviceTypeRequestValidator() { RuleFor(x => x.FingerPrintDeviceTypeID).GreaterThan(0); } }
}
