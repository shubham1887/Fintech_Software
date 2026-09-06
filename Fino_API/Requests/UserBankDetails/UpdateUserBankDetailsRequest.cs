using FluentValidation;
namespace FintechApp.Requests.UserBankDetails
{
    public class UpdateUserBankDetailsRequest { public int UserBankDetailsID { get; set; } public int UserID { get; set; } public string BankName { get; set; } = string.Empty; public string AccountNumber { get; set; } = string.Empty; public string IFSC { get; set; } = string.Empty; public bool IsOn { get; set; } public string HolderName { get; set; } = string.Empty; public string Branch { get; set; } = string.Empty; public string? QRCodeImageURL { get; set; } public string? UPINumber { get; set; } }
    public class UpdateUserBankDetailsRequestValidator : AbstractValidator<UpdateUserBankDetailsRequest> { public UpdateUserBankDetailsRequestValidator() { RuleFor(x => x.UserBankDetailsID).GreaterThan(0); } }
}
