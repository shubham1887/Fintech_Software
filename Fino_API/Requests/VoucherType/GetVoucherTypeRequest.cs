using FluentValidation;
namespace FintechApp.Requests.VoucherType
{
    public class GetVoucherTypeRequest { public int VoucherTypeID { get; set; } }
    public class GetVoucherTypeRequestValidator : AbstractValidator<GetVoucherTypeRequest> { public GetVoucherTypeRequestValidator() { RuleFor(x => x.VoucherTypeID).GreaterThan(0); } }
}
