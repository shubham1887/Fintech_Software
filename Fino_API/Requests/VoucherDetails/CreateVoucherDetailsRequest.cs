using FluentValidation;
namespace FintechApp.Requests.VoucherDetails
{
    public class CreateVoucherDetailsRequest { public int UserID { get; set; } public string? Mode { get; set; } public decimal OutstandingOpeningBal { get; set; } public decimal Amount { get; set; } public decimal OutstandingClossingBal { get; set; } public string? Remark { get; set; } public string? Credit { get; set; } public string? Debit { get; set; } }
    public class CreateVoucherDetailsRequestValidator : AbstractValidator<CreateVoucherDetailsRequest> { public CreateVoucherDetailsRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
