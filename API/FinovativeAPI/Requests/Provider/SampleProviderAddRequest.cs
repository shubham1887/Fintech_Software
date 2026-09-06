using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Provider
{
    public class SampleProviderAddRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _ProviderDetailsSampleID;

        public int ProviderDetailsSampleID
        {
            get => _ProviderDetailsSampleID;
            set => _ProviderDetailsSampleID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SampleProviderAddRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SampleProviderAddRequestValidator : AbstractValidator<SampleProviderAddRequest>
    {
        public SampleProviderAddRequestValidator()
        {
            RuleFor(x => x.ProviderID)
               .Must(new Validation().IsValidInt)
               .WithMessage(ProviderMessage.InValidProvider);

            RuleFor(x => x.ProviderDetailsSampleID)
               .Must(new Validation().IsValidInt)
               .WithMessage(ProviderResponseMessage.InvalidSampleResponse);
        }
    }
}