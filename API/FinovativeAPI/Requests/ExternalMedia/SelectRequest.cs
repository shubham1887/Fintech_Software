using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ExternalMedia
{
    public class SelectRequest
    {
        private string _Key;

        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.Key)
           .Must(new Validation().IsValidString)
           .WithMessage(UserBankDetailsMessage.InvalidKey);
        }
    }
}
