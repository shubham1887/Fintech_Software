using FluentValidation;

namespace FintechApp.Requests.KycDocumentMaster
{
    public class DeleteKycDocumentMasterRequest
    {
        public int KycDocumentMasterID { get; set; }
    }

    public class DeleteKycDocumentMasterRequestValidator : AbstractValidator<DeleteKycDocumentMasterRequest>
    {
        public DeleteKycDocumentMasterRequestValidator()
        {
            RuleFor(x => x.KycDocumentMasterID).GreaterThan(0);
        }
    }
}
