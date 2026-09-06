using FluentValidation;
namespace FintechApp.Requests.VoucherType
{
    public class UpdateVoucherTypeRequest { public int VoucherTypeID { get; set; } public string? VoucherTypeName { get; set; } }
    public class UpdateVoucherTypeRequestValidator : AbstractValidator<UpdateVoucherTypeRequest> { public UpdateVoucherTypeRequestValidator() { RuleFor(x => x.VoucherTypeID).GreaterThan(0); } }
}
