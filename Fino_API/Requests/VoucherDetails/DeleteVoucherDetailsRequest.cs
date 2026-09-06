using FluentValidation;
namespace FintechApp.Requests.VoucherDetails
{
    public class DeleteVoucherDetailsRequest { public int VoucherDetailsID { get; set; } }
    public class DeleteVoucherDetailsRequestValidator : AbstractValidator<DeleteVoucherDetailsRequest> { public DeleteVoucherDetailsRequestValidator() { RuleFor(x => x.VoucherDetailsID).GreaterThan(0); } }
}
