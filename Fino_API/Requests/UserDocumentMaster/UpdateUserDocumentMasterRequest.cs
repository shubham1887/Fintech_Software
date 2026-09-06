using FluentValidation;
namespace FintechApp.Requests.UserDocumentMaster
{
    public class UpdateUserDocumentMasterRequest { public int UserDocumentID { get; set; } public int UserID { get; set; } public string DocmentType { get; set; } = string.Empty; public string DocumentNo { get; set; } = string.Empty; public string ImageURL { get; set; } = string.Empty; public string? Remark { get; set; } public string? Status { get; set; } }
    public class UpdateUserDocumentMasterRequestValidator : AbstractValidator<UpdateUserDocumentMasterRequest> { public UpdateUserDocumentMasterRequestValidator() { RuleFor(x => x.UserDocumentID).GreaterThan(0); } }
}
