using FluentValidation;
namespace FintechApp.Requests.UserBankDetails
{
    public class CreateUserBankDetailsRequest { public int UserID { get; set; } public string BankName { get; set; } = string.Empty; public string AccountNumber { get; set; } = string.Empty; public string IFSC { get; set; } = string.Empty; public bool IsOn { get; set; } public string HolderName { get; set; } = string.Empty; public string Branch { get; set; } = string.Empty; public string? QRCodeImageURL { get; set; } public string? UPINumber { get; set; } }
    public class CreateUserBankDetailsRequestValidator : AbstractValidator<CreateUserBankDetailsRequest> { public CreateUserBankDetailsRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.AccountNumber).NotEmpty(); RuleFor(x => x.IFSC).NotEmpty(); } }
}
