using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.TicketMaster
{
    public class UpdateRequest
    {
        private int _TicketMasterID;

        public int TicketMasterID
        {
            get => _TicketMasterID;
            set => _TicketMasterID = value;
        }

        private string _Body;

        public string Body
        {
            get => _Body.TrimIfNotNull();
            set => _Body = value.TrimIfNotNull();
        }

        private string _Subject;

        public string Subject
        {
            get => _Subject.TrimIfNotNull();
            set => _Subject = value.TrimIfNotNull();
        }

        public CustomFile Document = new CustomFile();

        private int _TicketCategoryID;

        public int TicketCategoryID
        {
            get => _TicketCategoryID;
            set => _TicketCategoryID = value;
        }

        internal string IsValidModalStateForDocument()
        {
            var validator = new FileUpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.Subject)
              .Must(new Validation().IsValidString)
              .WithMessage(Message.TicketMessage.InvalidSubject);

            RuleFor(x => x.Body)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.TicketMessage.InvalidBody);
            RuleFor(x => x.TicketCategoryID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(Message.TicketMessage.InvalidTicketCategory);
        }
    }

    internal class FileUpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public FileUpdateRequestValidator()
        {
            RuleFor(x => x.Document.Size).NotEmpty()
                 .Must(new Validation().IsValidFileOrImageSize)
                 .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.Document.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);
        }
    }
}