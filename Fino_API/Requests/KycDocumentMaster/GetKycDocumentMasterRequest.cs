using FluentValidation;

namespace FintechApp.Requests.KycDocumentMaster
{
    public class GetKycDocumentMasterRequest
    {
        public int KycDocumentMasterID { get; set; }
    }

    public class GetKycDocumentMasterRequestValidator : AbstractValidator<GetKycDocumentMasterRequest>
    {
        public GetKycDocumentMasterRequestValidator()
        {
            RuleFor(x => x.KycDocumentMasterID).GreaterThan(0);
        }
    }
}
