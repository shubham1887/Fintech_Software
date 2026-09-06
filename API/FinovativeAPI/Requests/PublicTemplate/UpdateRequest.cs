using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PublicTemplate
{
    public class UpdateRequest
    {
        private string _Key;

        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }

        private string _Value;

        public string Value
        {
            get => _Value.TrimIfNotNull();
            set => _Value = value.TrimIfNotNull();
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
            RuleFor(x => x.Key)
                      .Must(new Validation().IsValidString)
                      .WithMessage(PublicTemplateMessages.InvalidKey);
            RuleFor(x => x.Value)
                                .Must(new Validation().IsValidString)
                                .WithMessage(PublicTemplateMessages.InvalidValue);
        }
    }
}