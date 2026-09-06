using FluentValidation;
namespace FintechApp.Requests.EcollectionDetails
{
    public class UpdateEcollectionDetailsRequest { public int EcollectionDetailsID { get; set; } public int UserID { get; set; } public int WalletID { get; set; } public string VirtualACCode { get; set; } = string.Empty; public string UTR { get; set; } = string.Empty; public string PayeeAccountNumber { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string Reason { get; set; } = string.Empty; public decimal Amount { get; set; } }
    public class UpdateEcollectionDetailsRequestValidator : AbstractValidator<UpdateEcollectionDetailsRequest> { public UpdateEcollectionDetailsRequestValidator() { RuleFor(x => x.EcollectionDetailsID).GreaterThan(0); } }
}
