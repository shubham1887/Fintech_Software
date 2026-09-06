using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderResponse
{
    public class TestResponseRequest
    {
        private string _TextMustExists;

        public string TextMustExists
        {
            get => _TextMustExists.TrimIfNotNull();
            set => _TextMustExists = value.TrimIfNotNull();
        }

        private string _TextCompare;

        public string TextCompare
        {
            get => _TextCompare.TrimIfNotNull();
            set => _TextCompare = value.TrimIfNotNull();
        }

        private string _OriginalText;

        public string OriginalText
        {
            get => _OriginalText.TrimIfNotNull();
            set => _OriginalText = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal class InsertRequestValidator : AbstractValidator<TestResponseRequest>
        {
            public InsertRequestValidator()
            {
                RuleFor(x => x.TextMustExists)
                    .Must(new Validation().IsValidString)
                    .WithMessage(ProviderResponseMessage.TextValid);

                RuleFor(x => x.TextCompare)
                   .Must(new Validation().IsValidString)
                   .WithMessage(ProviderResponseMessage.TextValid);

                RuleFor(x => x.OriginalText)
                    .Must(new Validation().IsValidString)
                    .WithMessage(ProviderResponseMessage.TextValid);
            }
        }
    }
}