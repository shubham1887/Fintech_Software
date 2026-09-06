using FluentValidation;
namespace FintechApp.Requests.VoucherDetails
{
    public class GetVoucherDetailsRequest { public int VoucherDetailsID { get; set; } public int UserID { get; set; } }
    public class GetVoucherDetailsRequestValidator : AbstractValidator<GetVoucherDetailsRequest> { public GetVoucherDetailsRequestValidator() { RuleFor(x => x.VoucherDetailsID).GreaterThan(0); } }
}
