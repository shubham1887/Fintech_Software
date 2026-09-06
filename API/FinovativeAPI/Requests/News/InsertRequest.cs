using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.News
{
    public class InsertRequest
    {
        private string _Title;

        public string Title
        {
            get => _Title.TrimIfNotNull();
            set => _Title = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _HyperLink;

        public string HyperLink
        {
            get => _HyperLink.TrimIfNotNull();
            set => _HyperLink = value.TrimIfNotNull();
        }

        private List<UserTypeList> _list;

        public List<UserTypeList> UserTypeList
        {
            get => _list;
            set => _list = value;
        }

        public CustomFile Document = new CustomFile();

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal string IsValidModalStateForDocument()
        {
            var validator = new FileValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.Title)
                      .Must(new Validation().IsValidString)
                      .WithMessage(NewsMessage.InvalidTitle);

            RuleFor(x => x.Description)
                      .Must(new Validation().IsValidString)
                      .WithMessage(NewsMessage.InvalidDescription);
        }
    }

    internal class FileValidator : AbstractValidator<InsertRequest>
    {
        public FileValidator()
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