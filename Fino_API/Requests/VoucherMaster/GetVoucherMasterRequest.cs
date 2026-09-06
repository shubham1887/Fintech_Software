using FluentValidation;
namespace FintechApp.Requests.VoucherMaster
{
    public class GetVoucherMasterRequest { public int VoucherMasterID { get; set; } public int UserID { get; set; } }
    public class GetVoucherMasterRequestValidator : AbstractValidator<GetVoucherMasterRequest> { public GetVoucherMasterRequestValidator() { RuleFor(x => x.VoucherMasterID).GreaterThan(0); } }
}
