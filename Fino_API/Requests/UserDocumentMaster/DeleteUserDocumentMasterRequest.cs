using FluentValidation;
namespace FintechApp.Requests.UserDocumentMaster
{
    public class DeleteUserDocumentMasterRequest { public int UserDocumentID { get; set; } }
    public class DeleteUserDocumentMasterRequestValidator : AbstractValidator<DeleteUserDocumentMasterRequest> { public DeleteUserDocumentMasterRequestValidator() { RuleFor(x => x.UserDocumentID).GreaterThan(0); } }
}
