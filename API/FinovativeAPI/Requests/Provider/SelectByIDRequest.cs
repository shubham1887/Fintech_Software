using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Provider
{
    public class SelectByIDRequest
    {
        private int _ProviderDetailsSampleID;

        public int ProviderDetailsSampleID
        {
            get => _ProviderDetailsSampleID;
            set => _ProviderDetailsSampleID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.ProviderDetailsSampleID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(Message.ProviderMessage.InValidProvider);
        }
    }
}