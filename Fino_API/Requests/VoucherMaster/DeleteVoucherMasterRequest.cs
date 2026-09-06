using FluentValidation;
namespace FintechApp.Requests.VoucherMaster
{
    public class DeleteVoucherMasterRequest { public int VoucherMasterID { get; set; } }
    public class DeleteVoucherMasterRequestValidator : AbstractValidator<DeleteVoucherMasterRequest> { public DeleteVoucherMasterRequestValidator() { RuleFor(x => x.VoucherMasterID).GreaterThan(0); } }
}
