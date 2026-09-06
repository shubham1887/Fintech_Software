using FluentValidation;

namespace FintechApp.Requests.KycDocumentMaster
{
    public class CreateKycDocumentMasterRequest
    {
        public string? Label { get; set; }
        public string? DocumentType { get; set; }
        public string? Remark { get; set; }
        public bool IsRequired { get; set; }
        public string? RegularRxpression { get; set; }
        public string? SampleImageURL { get; set; }
    }

    public class CreateKycDocumentMasterRequestValidator : AbstractValidator<CreateKycDocumentMasterRequest>
    {
        public CreateKycDocumentMasterRequestValidator()
        {
            RuleFor(x => x.Label).NotEmpty();
        }
    }
}
