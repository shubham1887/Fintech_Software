using FluentValidation;
namespace FintechApp.Requests.GSTHold
{
    public class UpdateGSTHoldRequest { public int GSTHoldID { get; set; } public int UserID { get; set; } public int Month { get; set; } public int Year { get; set; } public decimal HoldAmount { get; set; } public DateTime GSTUploadedDate { get; set; } = DateTime.MinValue; public string ReceiptPath { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string GSTUploadRemark { get; set; } = string.Empty; public string AdminRemark { get; set; } = string.Empty; public string GSTNo { get; set; } = string.Empty; public int StateID { get; set; } public decimal TaxableValue { get; set; } public decimal IGST { get; set; } public decimal CGST { get; set; } public decimal SGST { get; set; } }
    public class UpdateGSTHoldRequestValidator : AbstractValidator<UpdateGSTHoldRequest> { public UpdateGSTHoldRequestValidator() { RuleFor(x => x.GSTHoldID).GreaterThan(0); } }
}
