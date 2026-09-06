using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.Requests.KYC
{
    public class UploadRequest
    {
        private string _DocumentType;
        private string _DocumentNo;

        public string DocumentType
        {
            get => _DocumentType.TrimIfNotNull();
            set => _DocumentType = value.TrimIfNotNull();
        }

        public string DocumentNo
        {
            get => _DocumentNo.TrimIfNotNull();
            set => _DocumentNo = value.TrimIfNotNull();
        }

        public CustomFile Document = new CustomFile();

        private int _KycDocumentMasterID;

        public int KycDocumentMasterID
        {
            get => _KycDocumentMasterID;
            set => _KycDocumentMasterID = value;
        }

        private int _SelectedUserID;

        public int SelectedUserID
        {
            get => _SelectedUserID;
            set => _SelectedUserID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UploadRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures.FirstOrDefault().ToString();
        }
    }

    internal class UploadRequestValidator : AbstractValidator<UploadRequest>
    {
        private readonly Validation validation = new Validation();

        public UploadRequestValidator()
        {
            RuleFor(x => x.DocumentNo)
              .Must(validation.IsValidString)
              .WithMessage(Message.KYCMessage.InvalidDocumentNo);

            RuleFor(x => x.KycDocumentMasterID)
               .Must(validation.IsValidInt)
               .WithMessage(Message.KYCMessage.InvalidDocumentType);

            RuleFor(x => x.Document.Size).NotEmpty()
               .Must(new Validation().IsValidFileOrImageSize)
               .WithMessage(Message.KYCMessage.InvalidFileSize);

            RuleFor(x => x.Document.Data).NotEmpty()
              .Must(new Validation().IsValidFileSize)
              .WithMessage(Message.KYCMessage.InvalidFileSize);

            RuleFor(x => x.Document.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.KYCMessage.InvalidFileType);
        }
    }
}