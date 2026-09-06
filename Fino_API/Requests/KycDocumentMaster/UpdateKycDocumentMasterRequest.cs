using FluentValidation;

namespace FintechApp.Requests.KycDocumentMaster
{
    public class UpdateKycDocumentMasterRequest
    {
        public int KycDocumentMasterID { get; set; }
        public string? Label { get; set; }
        public string? DocumentType { get; set; }
        public string? Remark { get; set; }
        public bool IsRequired { get; set; }
        public string? RegularRxpression { get; set; }
        public string? SampleImageURL { get; set; }
    }

    public class UpdateKycDocumentMasterRequestValidator : AbstractValidator<UpdateKycDocumentMasterRequest>
    {
        public UpdateKycDocumentMasterRequestValidator()
        {
            RuleFor(x => x.KycDocumentMasterID).GreaterThan(0);
            RuleFor(x => x.Label).NotEmpty();
        }
    }
}
