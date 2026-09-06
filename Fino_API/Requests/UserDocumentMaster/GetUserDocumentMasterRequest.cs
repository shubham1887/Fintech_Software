using FluentValidation;
namespace FintechApp.Requests.UserDocumentMaster
{
    public class GetUserDocumentMasterRequest { public int UserDocumentID { get; set; } public int UserID { get; set; } }
    public class GetUserDocumentMasterRequestValidator : AbstractValidator<GetUserDocumentMasterRequest> { public GetUserDocumentMasterRequestValidator() { RuleFor(x => x.UserDocumentID).GreaterThan(0); } }
}
