using FluentValidation;
namespace FintechApp.Requests.UserDocumentMaster
{
    public class CreateUserDocumentMasterRequest { public int UserID { get; set; } public string DocmentType { get; set; } = string.Empty; public string DocumentNo { get; set; } = string.Empty; public string ImageURL { get; set; } = string.Empty; public string? Remark { get; set; } public string? Status { get; set; } }
    public class CreateUserDocumentMasterRequestValidator : AbstractValidator<CreateUserDocumentMasterRequest> { public CreateUserDocumentMasterRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.DocumentNo).NotEmpty(); } }
}
