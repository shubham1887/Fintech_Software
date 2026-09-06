using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RichTextMedia
{
    public class SelectMediaKeyRequest
    {
        private string _Key;

        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectMediaRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectMediaRequestValidator : AbstractValidator<SelectMediaKeyRequest>
    {
        public SelectMediaRequestValidator()
        {
            RuleFor(x => x.Key)
                .Must(new Validation().IsValidString)
                .WithMessage(TextMediaMessage.InvalidKey);
        }
    }
}