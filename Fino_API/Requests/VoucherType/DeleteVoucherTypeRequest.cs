using FluentValidation;
namespace FintechApp.Requests.VoucherType
{
    public class DeleteVoucherTypeRequest { public int VoucherTypeID { get; set; } }
    public class DeleteVoucherTypeRequestValidator : AbstractValidator<DeleteVoucherTypeRequest> { public DeleteVoucherTypeRequestValidator() { RuleFor(x => x.VoucherTypeID).GreaterThan(0); } }
}
