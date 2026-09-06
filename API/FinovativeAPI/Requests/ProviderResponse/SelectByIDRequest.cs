using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderResponse
{
    public class SelectByIDRequest
    {
        private int _ProviderResponseMasterSampleID;

        public int ProviderResponseMasterSampleID
        {
            get => _ProviderResponseMasterSampleID;
            set => _ProviderResponseMasterSampleID = value;
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
            RuleFor(x => x.ProviderResponseMasterSampleID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(Message.ProviderResponseMessage.InvalidResponse);
        }
    }
}