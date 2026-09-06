using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Beneficiary
{
    public class UploadDocumentRequest
    {
        public CustomFile PanCard = new CustomFile();

        public CustomFile Passbook = new CustomFile();

        private string _BeneficiaryID;

        public string BeneficiaryID
        {
            get => _BeneficiaryID.TrimIfNotNull();
            set => _BeneficiaryID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UploadDocumentRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
    {
        public UploadDocumentRequestValidator()
        {
            RuleFor(x => x.BeneficiaryID)
            .Must(new Validation().IsValidString)
            .WithMessage(BeneficiaryMessage.InvalidBeneficiary);
        }
    }
}