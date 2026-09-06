using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.Requests.User
{
    public class ProfileUploadRequest
    {
        public CustomFile Document = new CustomFile();

        internal string IsValidModalState()
        {
            var validator = new ProfileUploadRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures.FirstOrDefault().ToString();
        }
    }

    internal class ProfileUploadRequestValidator : AbstractValidator<ProfileUploadRequest>
    {
        private readonly Validation validation = new Validation();

        public ProfileUploadRequestValidator()
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