using FluentValidation;
namespace FintechApp.Requests.VoucherMaster
{
    public class CreateVoucherMasterRequest { public int VoucherTypeID { get; set; } public DateTime SelectDate { get; set; } public decimal Amount { get; set; } public string? CrGroup { get; set; } public string? CrName { get; set; } public string? DrGroup { get; set; } public string? DrName { get; set; } public string? Remark { get; set; } public string? Status { get; set; } public int SubmitedBy { get; set; } public int ApproveBy { get; set; } public int UserID { get; set; } public bool Is_Delete { get; set; } }
    public class CreateVoucherMasterRequestValidator : AbstractValidator<CreateVoucherMasterRequest> { public CreateVoucherMasterRequestValidator() { RuleFor(x => x.VoucherTypeID).GreaterThan(0); RuleFor(x => x.UserID).GreaterThan(0); } }
}

