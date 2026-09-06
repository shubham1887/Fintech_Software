using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RichTextMedia
{
    public class UpdateRequest
    {
        private int _ID;

        public int ID
        {
            get => _ID;
            set => _ID = value;
        }

        private string _Key;

        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }

        private string _Content;

        public string Content
        {
            get => _Content.TrimIfNotNull();
            set => _Content = value.TrimIfNotNull();
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
            RuleFor(x => x.ID)
                .Must(new Validation().IsValidInt)
                .WithMessage(TextMediaMessage.InvalidID);

            RuleFor(x => x.Key)
               .Must(new Validation().IsValidString)
               .WithMessage(TextMediaMessage.InvalidKey);

            RuleFor(x => x.Content)
             .Must(new Validation().IsValidString)
             .WithMessage(TextMediaMessage.InvalidContent);
        }
    }
}