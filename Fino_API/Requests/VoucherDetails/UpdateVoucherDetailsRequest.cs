using FluentValidation;
namespace FintechApp.Requests.VoucherDetails
{
    public class UpdateVoucherDetailsRequest { public int VoucherDetailsID { get; set; } public int UserID { get; set; } public string? Mode { get; set; } public decimal OutstandingOpeningBal { get; set; } public decimal Amount { get; set; } public decimal OutstandingClossingBal { get; set; } public string? Remark { get; set; } public string? Credit { get; set; } public string? Debit { get; set; } }
    public class UpdateVoucherDetailsRequestValidator : AbstractValidator<UpdateVoucherDetailsRequest> { public UpdateVoucherDetailsRequestValidator() { RuleFor(x => x.VoucherDetailsID).GreaterThan(0); } }
}
